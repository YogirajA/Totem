<template>
  <transition @leave="onModalHide" @enter="onModalShow">
    <ModalWindow
      ref="modalWindow"
      :title="modalTitle"
      :success-btn="successBtn"
      :cancel-btn="cancelBtn"
      :delete-btn="deleteBtn"
      :is-editing="isEditing"
      :disable-delete="disableDelete"
    >
      <template v-slot:body>
        <div class="form-group">
          <label for="fieldName" class="control-label">Property Name</label>
          <input
            id="propertyName"
            v-model="fieldName"
            autocomplete="off"
            class="field-name form-control"
            placeholder="Property Name"
          />
        </div>
        <div class="form-group">
          <label for="selectdata" class="control-label">Data Model</label>
          <multiselect
            ref="propertyType"
            v-model="fieldType"
            :options="options"
            label="displayName"
            track-by="id"
            :searchable="true"
            :close-on-select="true"
            :show-labels="false"
            placeholder="Select..."
            @input="onChange"
          />
        </div>
        <div class="form-check form-group">
          <input
            id="isArray"
            v-model="isArray"
            class="form-check-input"
            type="checkbox"
            @change="onCheckboxChange"
          />
          <label for="isArray" class="control-label">Array</label>
        </div>
        <div class="form-group">
          <label for="fieldexample" class="control-label">Example</label>
          <input
            id="propertyExample"
            v-model="example"
            class="form-control"
            placeholder="Example"
            :disabled="isExampleDisabled"
          />
        </div>
      </template>
    </ModalWindow>
  </transition>
</template>

<script>
import $ from 'jquery';
import Multiselect from 'vue-multiselect';
import ModalWindow from '../../components/ModalWindow.vue';
import { buildNewObject } from './contractParser';
import { deepCopy, last } from './dataHelpers';

export default {
  name: 'AddNewFieldModalWindow',
  components: {
    ModalWindow,
    Multiselect
  },
  props: {
    title: { type: String, default: '' },
    name: { type: String, default: '' },
    change: { type: Function, default: () => {} },
    options: { type: Array, default: () => [] },
    editStack: { type: Array, default: () => [] },
    isDescending: { type: Boolean, default: false },
    modifiedContract: { type: String, default: '' },
    disableDelete: { type: Boolean, default: false }
  },
  data() {
    return {
      fieldName: this.name,
      fieldType: null,
      example: null,
      modalTitle: this.title,
      isArray: false,
      isExampleDisabled: false,
      successBtn: {
        id: 'saveFieldBtn',
        text: 'Add New Field',
        clicked: this.saveField
      },
      cancelBtn: {
        text: 'Cancel',
        clicked: this.close
      },
      deleteBtn: {
        id: 'deleteFieldBtn',
        text: 'Delete',
        clicked: this.deleteField
      }
    };
  },
  computed: {
    isEditing() {
      let isEditing = false;
      if (last(this.editStack)) {
        const currentField = deepCopy(last(this.editStack));
        isEditing = currentField.rowId !== undefined || currentField.modalRowId !== undefined;
      }
      return isEditing;
    }
  },
  methods: {
    onChange(option) {
      const currentField = last(this.editStack);
      if (option.id === 0) {
        // Defining a new model; Open the Add Model window
        const newModel = {
          name: this.fieldName,
          type: this.isArray ? 'array' : 'object',
          items: this.isArray ? { properties: [] } : undefined,
          properties: this.isArray ? undefined : []
        };
        if (this.isEditing) {
          newModel.rowId = currentField.rowId;
        }
        this.$emit('showModelWindow', newModel, true);
      } else {
        // Set the "example" field based on the selected model
        const schemaObj = JSON.parse(option.value.schemaString);
        this.updateSchemaExample(schemaObj, option && option.isObject);
      }
    },
    onCheckboxChange() {
      if (this.fieldType) {
        const schemaObj = JSON.parse(this.fieldType.value.schemaString);
        this.updateSchemaExample(schemaObj, this.fieldType.isObject);
      }
    },
    updateSchemaExample(schemaObj, isObject, explicitExample) {
      this.isExampleDisabled = isObject === true;
      if (isObject === true) {
        this.example = null;
      } else if (explicitExample) {
        this.example = explicitExample;
      } else if (this.isArray === true) {
        const type = schemaObj.type === 'array' ? schemaObj.items.type : schemaObj.type;
        this.example = type === 'string' ? `["${schemaObj.example}"]` : `[${schemaObj.example}]`;
      } else {
        this.example = schemaObj.example;
      }
    },
    onModalShow() {
      this.successBtn = {
        id: 'saveFieldBtn',
        text: 'Add New Field',
        clicked: this.saveField
      };
      this.modalTitle = this.title;
      const currentField = deepCopy(last(this.editStack));
      const editingExistingRow = currentField.name !== undefined;

      if (editingExistingRow || this.isDescending) {
        // Editing an existing field or moving up to a parent object
        this.fieldName = currentField.name;
        this.example = currentField.example;
        this.isArray = currentField.type === 'array';
        const currentFieldNodeToUse = this.isArray === true ? currentField.items : currentField;
        let typeName = currentFieldNodeToUse.type;

        if (this.isDescending) {
          typeName = currentFieldNodeToUse.name || '';
        } else if (currentFieldNodeToUse.reference) {
          typeName = currentFieldNodeToUse.reference;
        } else if (currentFieldNodeToUse.properties && currentFieldNodeToUse.type === undefined) {
          typeName = currentFieldNodeToUse.name;
        } else if (
          typeName.toLowerCase() === 'string' &&
          currentFieldNodeToUse.format &&
          currentFieldNodeToUse.format.toLowerCase() === 'date-time'
        ) {
          typeName = 'DateTime';
        } else if (typeName.toLowerCase() === 'integer' && currentFieldNodeToUse.format) {
          typeName = `${typeName.toLowerCase()}(${currentFieldNodeToUse.format})`;
        }
        this.fieldType = this.options.find(
          option => option.displayName.toLowerCase() === typeName.toLowerCase()
        );
        this.updateSchemaExample(
          currentFieldNodeToUse,
          this.fieldType && this.fieldType.isObject,
          currentFieldNodeToUse.example
        );

        if (editingExistingRow) {
          this.successBtn = {
            id: 'saveFieldBtn',
            text: 'Update Field',
            clicked: this.saveField
          };
          this.modalTitle = 'Update Field';
        }
      } else {
        this.updateSchemaExample(currentField, false);
      }
      $('.field-name')[0].focus();
    },
    onModalHide() {
      this.example = null;
      this.fieldName = null;
      this.fieldType = null;
      this.isArray = false;
    },
    deleteField() {
      this.$emit('delete');
    },
    close() {
      this.$emit('close', 'addField', false, true);
    },
    saveField() {
      // Name and type are required to save the field
      if (this.fieldName !== '' && this.fieldType !== null) {
        const field = deepCopy(last(this.editStack));
        this.$emit(
          'save',
          buildNewObject(
            this.fieldName,
            this.fieldType,
            this.isArray,
            this.example,
            field,
            this.modifiedContract
          )
        );
      }
    }
  }
};
</script>
