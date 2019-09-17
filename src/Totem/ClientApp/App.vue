<template>
  <div id="contract-list">
    <ContractGrid
      ref="rootContractGrid"
      :rows="rows"
      :hide-ellipsis-menu="false"
      :edit-stack="editStack"
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
      :parent-name="currentParentName"
      :edit-stack="editStack"
      @showFieldWindow="showFieldWindow(...arguments)"
      @close="closeModal(...arguments)"
      @save="saveModel(...arguments)"
      @delete="deleteModel(...arguments)"
    />
    <AddNewFieldModalWindow
      v-show="isAddFieldWindowVisible"
      ref="addNewFieldModalWindow"
      title="Add New Field"
      :name="currentName"
      :options="options"
      :edit-stack="editStack"
      :is-descending="isDescending"
      :modified-contract="modifiedContract"
      :disable-delete="disableDelete"
      @close="closeModal(...arguments)"
      @save="saveField(...arguments)"
      @delete="deleteField(...arguments)"
      @showModelWindow="showModelWindow(...arguments)"
    />
  </div>
</template>

<script>
import $ from 'jquery';
import _ from 'lodash';
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
import {
  reorderOptions,
  deepCopy,
  getUniqueId,
  findRowInTreeAndUpdate,
  last
} from './features/contracts/dataHelpers';

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
      parentName: '',
      editStack: [],
      modalRows: [],
      isDescending: false, // Descending = editStack is decreasing, returning to parent; not descending = adding new children
      modifiedContract,
      options: [],
      isEditManuallyWindowVisible: false,
      isAddFieldWindowVisible: false,
      isAddModelWindowVisible: false,
      disableDelete: false
    };
  },
  computed: {
    currentName() {
      return this.editStack.length > 0 ? last(this.editStack).name : '';
    },
    currentParentName: {
      get() {
        return this.parentName;
      },
      set(val) {
        if (val === '') {
          this.parentName = this.editStack.length > 0 ? last(this.editStack).name : '';
        } else {
          this.parentName = val;
        }
      }
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
      if (!_.isEmpty(field) && this.editStack.length === 0) {
        this.disableDelete = this.rows.some(row => {
          // Can not delete the only child of a parent model
          if (
            row.rowId === field.rowId + 1 &&
            row.type === 'object' &&
            row.properties.length === 1
          ) {
            return true;
          }
          return false;
        });
      }

      if (!_.isEmpty(field)) {
        // Editing a row that already exists
        const deepCopyField = deepCopy(field);
        if (deepCopyField.type === 'object' && deepCopyField.parentId !== undefined) {
          // Shouldn't happen here; ContractGrid should call showModalWindow instead
        } else {
          this.editStack.push({ ...deepCopyField });
        }
      } else {
        // Adding a new field at the root
        this.editStack.push({ parentId: null });
      }
      this.isEditManuallyWindowVisible = false;
      this.isAddFieldWindowVisible = true;
    },

    showModelWindow(row, isNewModel) {
      // If we push "row" as passed in, it maintains stale TreeGrid state (rows from cancelled edits)
      // so look up a clean row model from the root grid (if it exists)
      const previousModel = findRow(row.rowId, this.rows);
      const parentId = this.editStack.length === 0 ? null : last(this.editStack).rowId;

      const model = {
        name: row.name || (previousModel && previousModel.name),
        type: 'object',
        properties: row.properties || (previousModel && previousModel.properties) || [],
        rowId: row.rowId,
        isNewModel,
        parentId
      };

      if (previousModel && previousModel.modalRowId) {
        // Editing an existing model row
        model.modalRowId = row.modalRowId;
      }
      if (this.editStack.length > 0 && last(this.editStack).type !== 'object') {
        // Changing a row from a field to a model, so remove the field from the history
        this.editStack.pop();
      }

      this.currentParentName = model.name;
      this.editStack.push(deepCopy(model));
      this.modalRows = deepCopy(model.properties);
      this.isEditManuallyWindowVisible = false;
      this.isAddFieldWindowVisible = false;
      this.isAddModelWindowVisible = true;
    },

    closeModal(modal, alreadyAdjusted = false, setDescendingFalse = false) {
      if (modal === 'editManually') {
        this.isEditManuallyWindowVisible = false;
      } else if (modal === 'addField') {
        if (this.editStack.length > 0 && !alreadyAdjusted) {
          this.editStack.pop();
        }
        this.isAddFieldWindowVisible = false;
      } else if (modal === 'addModel') {
        if (this.editStack.length === 0) {
          // No parent objects to show, so reset and close the modal
          this.isDescending = setDescendingFalse;
          this.isAddModelWindowVisible = false;
          this.modalRows = [];
        } else {
          // Update the modal window to show the parent's rows
          this.isDescending = !setDescendingFalse;
          if (last(this.editStack).properties) {
            this.modalRows = deepCopy(last(this.editStack).properties);
            this.currentParentName = last(this.editStack).name;
          }
        }
      }
    },

    saveField(object) {
      const field = deepCopy(object);
      const addingToAModel = this.isAddModelWindowVisible;

      if (addingToAModel) {
        const isEditing = field.rowId !== undefined;
        if (isEditing) {
          // Row already exists in modalRows, so update it
          const parent = deepCopy(this.editStack[this.editStack.length - 2].properties);
          this.modalRows = updateNestedProperty(field, parent);
          this.editStack[this.editStack.length - 2].properties = deepCopy(this.modalRows);
        } else {
          // Creating a new row in the model
          if (field.rowId === undefined) {
            field.rowId = null; // allows editing of the row before it has an ID assigned
          }
          field.modalRowId = getUniqueId();
          if (this.modalRows.length > 0) {
            const rows = deepCopy(this.modalRows);
            rows.forEach(obj => {
              if (obj.name === this.parentName) {
                obj.properties.push(field);
              }
            });
            const parentObject = rows.find(obj => obj.name === this.parentName);
            if (parentObject) {
              this.modalRows = deepCopy(parentObject.properties);
            } else {
              this.modalRows.push(field);
            }

            this.editStack[this.editStack.length - 2].properties.push(field);
          } else {
            this.modalRows.push(field);
            this.editStack[this.editStack.length - 2].properties = deepCopy(this.modalRows);
          }
        }
      } else if (this.isDescending && this.editStack.length > 1) {
        // Update the modal window to show the parent's rows
        this.isAddModelWindowVisible = true;
        if (field.rowId === undefined) {
          field.rowId = null;
        }

        this.modalRows = this.modalRows.concat(
          this.editStack[this.editStack.length - 2].properties
        );
        this.modalRows.push(field);
        this.editStack[this.editStack.length - 2].properties = deepCopy(this.modalRows);
      } else {
        // Update the root object
        if (field.type === 'object') {
          this.options.push({
            displayName: field.name,
            id: this.options.length,
            value: {
              id: this.options.length,
              schemaName: field.name,
              schemaString: createSchemaString(field)
            }
          });
          this.options = reorderOptions(this.options);
        }

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

      if (updatedModel.isNewModel === true) {
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
      } else {
        const existingOption = this.options.find(option => option.displayName === model.name);
        if (existingOption) {
          existingOption.displayName = updatedModel.name;
          this.options = reorderOptions(this.options);
        }
      }
      this.editStack.pop();

      if (this.editStack.length > 0 && last(this.editStack).properties) {
        // Update the modal window to show the parent's rows
        this.modalRows = deepCopy(last(this.editStack).properties);
        updatedModel.modalRowId = getUniqueId();
        const updatedParent = findRowInTreeAndUpdate(this.modalRows, updatedModel);
        if (updatedParent) {
          this.modalRows = deepCopy(updatedParent);
        } else {
          // Existing model row ID not found, so add as a new row
          this.modalRows.push(updatedModel);
        }
        this.currentParentName = last(this.editStack).name;
        last(this.editStack).properties = deepCopy(this.modalRows);
      } else {
        // update root contract
        this.modifiedContract = updateContractString(
          updatedModel,
          this.rows,
          this.modifiedContract
        );
        $('#contract-raw')[0].value = JSON.stringify(JSON.parse(this.modifiedContract), null, 2);
        $('#ModifiedContract_ContractString')[0].value = this.modifiedContract;
        this.rows = parseContractArray(this.modifiedContract, 'contract-string-validation');
        this.isDescending = false;
        if (updatedModel.isNewModel && typeof setSaveButton === 'function') {
          // setSaveButton is defined in Edit.cshtml
          setSaveButton(); // eslint-disable-line no-undef
        }
        this.closeModal('addModel', true, false);
      }
    },

    deleteField() {
      const deepCopyObject = deepCopy(last(this.editStack));
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
        const deepCopyParent = deepCopy(this.editStack[this.editStack.length - 2].properties);
        this.modalRows = updateNestedProperty(deepCopyObject, deepCopyParent, true);
        this.editStack[this.editStack.length - 2].properties = deepCopy(this.modalRows);
      }
      this.closeModal('addField', false, true);
    },

    deleteModel(model) {
      const deepCopyModel = deepCopy(model);
      const deepCopyRows = deepCopy(this.rows);

      // Even if there were parent windows, close them all so that the deleted row can be removed properly
      this.editStack = [];

      // Update the root object
      this.modifiedContract = updateContractString(
        deepCopyModel,
        deepCopyRows,
        this.modifiedContract,
        true
      );
      $('#contract-raw')[0].value = JSON.stringify(JSON.parse(this.modifiedContract), null, 2);
      $('#ModifiedContract_ContractString')[0].value = this.modifiedContract;
      this.rows = parseContractArray(this.modifiedContract, 'contract-string-validation');
      this.closeModal('addModel', false, true);
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
