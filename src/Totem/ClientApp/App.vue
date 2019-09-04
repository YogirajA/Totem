<template>
  <div id="contract-list">
    <ContractGrid
      :rows="rows"
      :hide-ellipsis-menu="false"
      :partial-contract="partialContract"
      :current-index="currentIndex"
      @editManually="showEditManuallyWindow"
      @showFieldWindow="showFieldWindow(...arguments)"
      @showModelWindow="showModelWindow(...arguments)"
    />
    <EditContractModalWindow
      v-show="isEditManuallyWindowVisible"
      title="Edit Contract"
      :contract-string="modifiedContract"
      @close="closeModal('editManually')"
      @updateData="updateContractManually"
    />
    <AddModelModalWindow
      v-show="isAddModelWindowVisible"
      title="Add Model"
      :modal-rows="modalRows"
      :field-name="currentName"
      :partial-contract="partialContract"
      :current-index="currentIndex"
      @showFieldWindow="showFieldWindow(...arguments)"
      @close="closeModal(...arguments)"
      @save="saveModel(...arguments)"
      @delete="deleteModel(...arguments)"
    />
    <AddNewFieldModalWindow
      v-show="isAddFieldWindowVisible"
      title="Add New Field"
      :name="currentName"
      :options="options"
      :partial-contract="partialContract"
      :current-index="currentIndex"
      :is-descending="isDescending"
      :modified-contract="modifiedContract"
      @close="closeModal(...arguments)"
      @save="saveField(...arguments)"
      @delete="deleteField(...arguments)"
      @showModelWindow="showModelWindow(...arguments)"
    />
  </div>
</template>

<script>
import $ from 'jquery';
import EditContractModalWindow from './features/contracts/EditContractModalWindow.vue';
import AddNewFieldModalWindow from './features/contracts/AddNewFieldModalWindow.vue';
import AddModelModalWindow from './features/contracts/AddModelModalWindow.vue';
import ContractGrid from './features/contracts/ContractGrid.vue';
import {
  parseContractArray,
  updateContractString,
  createSchemaString,
  getExistingOptions,
  updateNestedProperty,
  findRow
} from './features/contracts/contractParser';
import { reorderOptions, deepCopy } from './features/contracts/dataHelpers';

export default {
  name: 'App',
  components: {
    ContractGrid,
    EditContractModalWindow,
    AddModelModalWindow,
    AddNewFieldModalWindow
  },
  data() {
    const modifiedContract = $('#ModifiedContract_ContractString').val();
    return {
      rows: parseContractArray(modifiedContract, 'contract-string-validation'),
      partialContract: [],
      modalRows: [],
      currentIndex: -1,
      isDescending: false,
      modifiedContract,
      options: [],
      isEditManuallyWindowVisible: false,
      isAddFieldWindowVisible: false,
      isAddModelWindowVisible: false
    };
  },
  computed: {
    currentName() {
      return this.currentIndex > -1 ? this.partialContract[this.currentIndex].name : '';
    }
  },
  created() {
    const { options } = this; // destructuring to give proper scope when used below
    $.ajax({
      url: '/contracts/schemaobjects',
      type: 'GET',
      contentType: 'application/json',
      success(result) {
        let existingOptions = getExistingOptions($('#ModifiedContract_ContractString').val());
        const defaultOptions = [];
        $.each(result, (idx, option) => {
          defaultOptions.push({
            id: option.id,
            displayName: option.schemaName,
            value: option
          });
        });
        existingOptions = existingOptions.concat(defaultOptions);
        const orderedOptions = reorderOptions(existingOptions);
        orderedOptions.forEach(option => {
          options.push(option);
        });
      }
    });
  },
  methods: {
    showEditManuallyWindow() {
      this.isAddFieldWindowVisible = false;
      this.isAddModelWindowVisible = false;
      this.isEditManuallyWindowVisible = true;
    },

    showFieldWindow(field) {
      // Show the edit field window
      if (!this.isDescending) {
        this.currentIndex += 1;
      }
      if (field) {
        // Editing a row that exists in a table
        const deepCopyField = deepCopy(field);
        if (deepCopyField.type === 'object' && deepCopyField.parentId !== undefined) {
          // Shouldn't happen here; ContractGrid should call showModalWindow instead
        } else if (this.isDescending) {
          // going back up the tree; values are already correct
        } else {
          this.partialContract[this.currentIndex] = { ...deepCopyField };
        }
      } else {
        // Adding a new field at the root
        this.partialContract[this.currentIndex] = { parentId: null };
      }
      this.isEditManuallyWindowVisible = false;
      this.isAddFieldWindowVisible = true;
    },

    showModelWindow(row) {
      // If we push "row" as passed in, it maintains stale TreeGrid state (rows from cancelled edits)
      // so look up a clean row model from the root grid (if it exists)
      let model = findRow(row.rowId, this.rows);
      if (model === undefined) {
        // Defining a new model
        model = {
          name: row.name,
          properties: []
        };
      }
      this.currentIndex += 1;
      const parentId =
        this.currentIndex === 0 ? null : this.partialContract[this.currentIndex - 1].rowId;
      model.parentId = parentId;
      this.partialContract.push(deepCopy(model));
      this.modalRows = deepCopy(model.properties);
      this.isEditManuallyWindowVisible = false;
      this.isAddFieldWindowVisible = false;
      this.isAddModelWindowVisible = true;
    },

    closeModal(modal, alreadyAdjusted = false, setDescendingFalse = false) {
      if (modal === 'editManually') {
        this.isEditManuallyWindowVisible = false;
      } else if (modal === 'addField') {
        if (this.currentIndex > -1 && !alreadyAdjusted) {
          this.partialContract.splice(this.currentIndex, 1);
          this.currentIndex -= 1;
        }
        this.isAddFieldWindowVisible = false;
      } else if (modal === 'addModel') {
        if (this.currentIndex > -1 && !alreadyAdjusted) {
          this.partialContract.splice(this.currentIndex, 1);
          this.currentIndex -= 1;
        }
        this.isDescending = !setDescendingFalse;
        this.isAddModelWindowVisible = false;
        this.modalRows = [];
      }
    },

    saveField(object) {
      const field = deepCopy(object);
      const addingToAModel = this.isAddModelWindowVisible;
      if (addingToAModel) {
        // Update the parent model
        const isEditing = field.rowId !== undefined;
        if (isEditing) {
          // Row already exists in modalRows, so update it
          const parent = deepCopy(this.partialContract[this.currentIndex - 1].properties);
          this.modalRows = updateNestedProperty(field, parent);
        } else {
          if (field.rowId === undefined) {
            field.rowId = null; // allows editing of the row before it has an ID assigned
          }
          field["modalRowId"] = this.modalRows.length + 1;
          this.modalRows.push(field);
        }
        this.partialContract[this.currentIndex - 1].properties = deepCopy(this.modalRows);
      } else {
        // Update the root object
        this.modifiedContract = updateContractString(field, this.rows, this.modifiedContract);
        $('#contract-raw')[0].value = JSON.stringify(JSON.parse(this.modifiedContract), null, 2);
        $('#ModifiedContract_ContractString')[0].value = this.modifiedContract;
        this.rows = parseContractArray(this.modifiedContract, 'contract-string-validation');
        this.isDescending = false; // End of the edit chain, so reset the edit direction
      }
      this.closeModal('addField', false, false);
    },

    saveModel(model, modelName = model.name) {
      const updatedModel = deepCopy(model);
      updatedModel.name = modelName;
      if (updatedModel.parentId === undefined) {
        // Add the newly added model name to the dropdown options
        this.options.push({
          displayName: updatedModel.name,
          id: this.options.length,
          value: {
            id: this.options.length,
            schemaName: updatedModel.name,
            schemaString: createSchemaString(updatedModel)
          }
        });
        this.options = reorderOptions(this.options);
      }
      const removedModel = this.partialContract.splice(this.currentIndex, 1);
      this.partialContract[this.currentIndex - 1] = Object.assign(
        { ...removedModel[0] },
        this.partialContract[this.currentIndex - 1]
      );
      if (this.currentIndex > 0) {
        // Skip the previous field edit window
        this.partialContract.splice(this.currentIndex - 1, 1);
        this.currentIndex -= 2;
      } else {
        this.currentIndex -= 1;
      }
      this.closeModal('addModel', true, false);
      if (this.currentIndex > -1) {
        this.showFieldWindow(updatedModel, false);
      } else {
        // update root contract
        this.modifiedContract = updateContractString(
          updatedModel,
          this.rows,
          this.modifiedContract
        );
        this.options.find(option => option.displayName == model.name).displayName = updatedModel.name;
        this.options = reorderOptions(this.options);
        $('#contract-raw')[0].value = JSON.stringify(JSON.parse(this.modifiedContract), null, 2);
        $('#ModifiedContract_ContractString')[0].value = this.modifiedContract;
        this.rows = parseContractArray(this.modifiedContract, 'contract-string-validation');
        this.isDescending = false;
      }
    },

    deleteField() {
      const deepCopyObject = deepCopy(this.partialContract[this.currentIndex]);
      const deepCopyRows = deepCopy(this.rows);
      const editingAModel = this.isAddModelWindowVisible;
      if (!editingAModel) {
        // Should only update the contract string if finished editing (only a field window showing)
        this.modifiedContract = updateContractString(
          deepCopyObject,
          deepCopyRows,
          this.modifiedContract,
          true
        );
        $('#contract-raw')[0].value = JSON.stringify(JSON.parse(this.modifiedContract), null, 2);
        $('#ModifiedContract_ContractString')[0].value = this.modifiedContract;
        this.rows = parseContractArray(this.modifiedContract, 'contract-string-validation');
      } else {
        // Update the parent model
        const deepCopyParent = deepCopy(this.partialContract[this.currentIndex - 1].properties);
        this.modalRows = updateNestedProperty(deepCopyObject, deepCopyParent, true);
        this.partialContract[this.currentIndex - 1].properties = deepCopy(this.modalRows);
      }
      this.closeModal('addField', false, true);
    },

    deleteModel(model) {
      const deepCopyModel = deepCopy(model);
      const deepCopyRows = deepCopy(this.rows);
      this.closeModal('addModel', false, true);
      this.modifiedContract = updateContractString(
        deepCopyModel,
        deepCopyRows,
        this.modifiedContract,
        true
      );
      $('#contract-raw')[0].value = JSON.stringify(JSON.parse(this.modifiedContract), null, 2);
      $('#ModifiedContract_ContractString')[0].value = this.modifiedContract;
      this.rows = parseContractArray(this.modifiedContract, 'contract-string-validation');
    },

    updateContractManually() {
      const newValue = $('#contract-raw')[0].value;
      $('#ModifiedContract_ContractString')[0].value = newValue;
      this.rows = parseContractArray(newValue, 'contract-string-validation');
      this.closeModal('editManually');
      $('#contract-raw').scrollTop(0);
    }
  }
};
</script>
