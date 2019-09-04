<template>
  <transition @leave="onModalHide" @enter="onModalShow">
    <ModalWindow
      :title="modalTitle"
      :success-btn="successBtn"
      :cancel-btn="cancelBtn"
      :delete-btn="deleteBtn"
      :is-editing="isEditing"
    >
      <template v-slot:body>
        <div class="form-group">
          <label for="fieldName" class="control-label">Property Name</label>
          <input v-model="fieldName" class="field-name form-control" placeholder="Property Name" />
        </div>
        <div class="form-group">
          <label for="selectdata" class="control-label">Data Model</label>
          <multiselect
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
        <div class="form-group">
          <label for="fieldexample" class="control-label">Example</label>
          <input v-model="example" class="form-control" placeholder="Example" />
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
import { deepCopy } from './dataHelpers';

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
    currentIndex: { type: Number, default: -1 },
    partialContract: { type: Array, default: () => [] },
    isDescending: { type: Boolean, default: false },
    modifiedContract: { type: String, default: '' }
  },
  data() {
    return {
      fieldName: this.name,
      fieldType: null,
      example: null,
      modalTitle: this.title,
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
      if (this.partialContract[this.currentIndex]) {
        const currentField = deepCopy(this.partialContract[this.currentIndex]);
        isEditing = currentField.rowId !== undefined;
      }
      return isEditing;
    }
  },
  methods: {
    onChange(option) {
      if (option.id === 0) {
        // Defining a new model; Open the Add Model window
        this.$emit('showModelWindow', {
          name: this.fieldName,
          type: 'object',
          properties: []
        });
      } else {
        // Set the "example" field based on the selected model
        const schemaObj = JSON.parse(option.value.schemaString);
        if (schemaObj.example) {
          this.example = schemaObj.example;
        }
      }
    },
    onModalShow() {
      this.successBtn = {
        id: 'saveFieldBtn',
        text: 'Add New Field',
        clicked: this.saveField
      };
      this.modalTitle = this.title;
      const currentField = deepCopy(this.partialContract[this.currentIndex]);
      const editingExistingRow = currentField.name !== undefined;
      if (editingExistingRow || this.isDescending) {
        // Editing an existing field or moving up through the chain
        this.fieldName = currentField.name;
        let typeName = currentField.type;
        if (this.isDescending) {
          typeName = currentField.name;
        } else if (currentField.reference) {
          typeName = currentField.reference;
        } else if (
          typeName.toLowerCase() === 'string' &&
          currentField.format &&
          currentField.format.toLowerCase() === 'date-time'
        ) {
          typeName = 'DateTime';
        }
        this.fieldType = this.options.find(
          option => option.displayName.toLowerCase() === typeName.toLowerCase()
        );
        this.example = currentField.example;
        if (editingExistingRow) {
          this.successBtn = {
            id: 'saveFieldBtn',
            text: 'Update Field',
            clicked: this.saveField
          };
          this.modalTitle = 'Update Field';
        }
      }
      $('.field-name')[0].focus();
    },
    onModalHide() {
      this.example = null;
      this.fieldName = null;
      this.fieldType = null;
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
        const field = deepCopy(this.partialContract[this.currentIndex]);
        this.$emit(
          'save',
          buildNewObject(this.fieldName, this.fieldType, this.example, field, this.modifiedContract)
        );
      }
    }
  }
};
</script>
