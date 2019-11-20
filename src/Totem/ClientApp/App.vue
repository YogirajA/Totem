<template>
  <div id="contract-list">
    <ContractGrid
      id="rootGrid"
      ref="rootContractGrid"
      :rows="rows"
      @editManually="showEditManuallyWindow"
      @importFromMessage="showImportWindow"
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
    <BuildContractFromMessageModalWindow
      v-show="isImportWindowVisible"
      title="Import Contract"
      @close="closeModal('importContract')"
      @importContract="importContract"
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
import BuildContractFromMessageModalWindow from './features/contracts/BuildContractFromMessageModalWindow.vue';
import AddNewFieldModalWindow from './features/contracts/AddNewFieldModalWindow.vue';
import AddModelModalWindow from './features/contracts/AddModelModalWindow.vue';
import ContractGrid from './features/contracts/ContractGrid.vue';
import {
  parseContractArray,
  updateContractString,
  createSchemaString,
  getExistingOptions,
  updateNestedProperty,
  findRow,
  buildContractFromMessage,
  getPropertiesCopy,
  isObjectArray,
  updateProperties,
  hasProperties
} from './features/contracts/contractParser';
import {
  reorderOptions,
  deepCopy,
  getUniqueId,
  findRowInTreeAndUpdate,
  last,
  findParent
} from './features/contracts/dataHelpers';

export default {
  name: 'App',
  components: {
    ContractGrid,
    EditContractModalWindow,
    BuildContractFromMessageModalWindow,
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
      isImportWindowVisible: false,
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
            value: option,
            isDefault: true
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
      this.isImportWindowVisible = false;
      this.isEditManuallyWindowVisible = true;
    },

    showImportWindow() {
      this.isAddFieldWindowVisible = false;
      this.isAddModelWindowVisible = false;
      this.isEditManuallyWindowVisible = false;
      this.isImportWindowVisible = true;
    },

    showFieldWindow(field) {
      // Show the edit field window
      if (!_.isEmpty(field) && this.editStack.length === 0) {
        this.disableDelete = this.rows.some(row => {
          // Can not delete the only child of a parent model
          if (
            row.rowId === field.rowId + 1 &&
            (row.type === 'object' || (row.items && row.items.type === 'object')) &&
            getPropertiesCopy(row).length === 1
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
      this.isImportWindowVisible = false;
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
        rowId: row.rowId,
        isNewModel,
        parentId
      };

      const properties = previousModel ? getPropertiesCopy(previousModel) : undefined;
      updateProperties(model, properties, isObjectArray(row));

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
      this.modalRows = getPropertiesCopy(model);
      this.isEditManuallyWindowVisible = false;
      this.isImportWindowVisible = false;
      this.isAddFieldWindowVisible = false;
      this.isAddModelWindowVisible = true;
    },

    closeModal(modal, alreadyAdjusted = false, setDescendingFalse = false) {
      if (modal === 'editManually') {
        this.isEditManuallyWindowVisible = false;
      } else if (modal === 'importContract') {
        this.isImportWindowVisible = false;
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
          const lastItem = last(this.editStack);
          if (hasProperties(lastItem)) {
            this.modalRows = getPropertiesCopy(lastItem);
            this.currentParentName = last(this.editStack).name;
          }
        }
      }
      this.updateSaveButtonState();
    },

    addNewModelToOptions(model) {
      this.options.push({
        displayName: model.name,
        id: this.options.length,
        value: {
          id: this.options.length,
          schemaName: model.name,
          schemaString: createSchemaString(model)
        },
        isObject: true
      });
      this.options = reorderOptions(this.options);
    },

    updateExistingOption(optionName, model) {
      const existingOption = this.options.find(option => option.displayName === optionName);
      if (existingOption) {
        existingOption.displayName = model.name;
        existingOption.value.schemaName = model.name;
        existingOption.value.schemaString = createSchemaString(model);
        this.options = reorderOptions(this.options);
      }
    },

    updateParent(field) {
      const parent = findParent(this.rows, field);
      if (parent) {
        const parentProperties = getPropertiesCopy(parent);
        const rowIndex = parentProperties.findIndex(prop => prop.rowId === field.rowId);
        parentProperties[rowIndex] = deepCopy(field);
        updateProperties(parent, parentProperties);
        this.updateExistingOption(parent.name, parent);
      }
    },

    saveField(object) {
      const field = deepCopy(object);
      const addingToAModel = this.isAddModelWindowVisible;
      const parentField = this.editStack[this.editStack.length - 2];
      const parentFieldProperties = parentField ? getPropertiesCopy(parentField) : undefined;

      if (addingToAModel) {
        // Update the parent model
        const isEditing = field.rowId !== undefined;
        if (isEditing) {
          // Row already exists in modalRows, so update it
          this.modalRows = updateNestedProperty(field, parentFieldProperties);
          updateProperties(parentField, deepCopy(this.modalRows));
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
                const properties = getPropertiesCopy(obj);
                properties.push(field);
                updateProperties(properties, field);
              }
            });
            const parentObject = rows.find(obj => obj.name === this.parentName);
            if (parentObject) {
              this.modalRows = getPropertiesCopy(parentObject);
            } else {
              this.modalRows.push(field);
            }

            parentFieldProperties.push(field);
            updateProperties(parentField, parentFieldProperties);
          } else {
            this.modalRows.push(field);
            updateProperties(parentField, deepCopy(this.modalRows));
          }
        }
      } else if (this.isDescending && this.editStack.length > 1) {
        // Update the modal window to show the parent's rows
        this.isAddModelWindowVisible = true;
        if (field.rowId === undefined) {
          field.rowId = null;
        }

        this.modalRows = this.modalRows.concat(parentFieldProperties);
        this.modalRows.push(field);
        updateProperties(parentField, deepCopy(this.modalRows));
      } else {
        // Update the root object
        if (field.type === 'object') {
          this.addNewModelToOptions(field);
        }

        this.updateParent(field);

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
        this.addNewModelToOptions(updatedModel);
      } else {
        this.updateExistingOption(model.name, updatedModel);
        this.updateParent(updatedModel);
        this.options = reorderOptions(this.options);
      }
      this.editStack.pop();

      if (this.editStack.length > 0 && hasProperties(last(this.editStack))) {
        // Update the modal window to show the parent's rows
        this.modalRows = getPropertiesCopy(last(this.editStack));
        const updatedParent = findRowInTreeAndUpdate(this.modalRows, updatedModel);
        updatedModel.modalRowId = getUniqueId();
        if (updatedParent) {
          this.modalRows = deepCopy(updatedParent);
        } else {
          // Existing model row ID not found, so add as a new row
          this.modalRows.push(updatedModel);
        }

        this.currentParentName = updatedModel.parentName
          ? updatedModel.parentName
          : last(this.editStack).name;
        updateProperties(last(this.editStack), deepCopy(this.modalRows));
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
        const parentField = this.editStack[this.editStack.length - 2];
        const deepCopyParent = getPropertiesCopy(parentField);
        this.modalRows = updateNestedProperty(deepCopyObject, deepCopyParent, true);
        updateProperties(parentField, deepCopy(this.modalRows));
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
    },

    addModelsToOptions(rows) {
      rows.forEach(row => {
        if (hasProperties(row)) {
          this.addNewModelToOptions(row);
          this.addModelsToOptions(getPropertiesCopy(row));
        }
      });
    },

    importContract() {
      const message = $('#import-message')[0].value;
      const contractBasedOnMessage = buildContractFromMessage(message);
      this.rows = parseContractArray(
        JSON.stringify(contractBasedOnMessage),
        'contract-string-validation'
      );

      this.options = this.options.filter(option => option.isDefault === true);
      this.addModelsToOptions(this.rows);

      $('#contract-raw')[0].value = JSON.stringify(contractBasedOnMessage, null, 2);
      $('#ModifiedContract_ContractString')[0].value = JSON.stringify(contractBasedOnMessage);
      this.closeModal('importContract');
      if (typeof setSaveButton === 'function') {
        // setSaveButton is defined in Edit.cshtml
        setSaveButton(); // eslint-disable-line no-undef
      }
      $('#contract-raw').scrollTop(0);
    },

    updateSaveButtonState() {
      /* eslint-disable */
      if (typeof setSaveButton === 'function') {
        // setSaveButton is defined in Create.cshtml and Edit.cshtml
        if (this.rows.length === 0 && setSaveButton.length > 0) {
          setSaveButton(true);
        } else {
          setSaveButton();
        }
      }
      /* eslint-enable */
    }
  }
};
</script>
