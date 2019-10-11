<template>
  <transition @enter="enter">
    <ModalWindow
      id="modelModalWindow"
      :title="`${computedModalTitle + ':  ' + modalFieldName}`"
      :success-btn="successBtn"
      :cancel-btn="cancelBtn"
      :class-name="`modal-scrollable`"
    >
      <template v-slot:body>
        <div class="container">
          <div class="row">
            <div class="form-group col-md-10">
              <label for="modelName" class="control-label">Property Name</label>
              <input
                id="modelName"
                v-model="modalFieldName"
                class="form-control"
                placeholder="Property Name"
              />
            </div>
            <div class="form-check form-group col-md-2 mt-auto">
              <input
                id="isObjectArray"
                v-model="isArray"
                class="form-check-input"
                type="checkbox"
                @change="onCheckboxChange"
              />
              <label for="isObjectArray" class="control-label">Array</label>
            </div>
          </div>
        </div>
        <ContractGrid
          id="nestedContractGrid"
          :rows="objectRows"
          :is-ellipsis-menu-visible="false"
          :is-import-button-visible="false"
          @showModelWindow="showModelWindow(...arguments)"
          @showFieldWindow="showFieldWindow(...arguments)"
          @editManually="this.$emit('showEditManuallyWindow')"
          @addModel="showModal('addModel', ...arguments)"
        />
      </template>
      <template v-if="isEditModal" v-slot:footer>
        <button
          :id="deleteBtn.id"
          type="button"
          class="ui-button btn btn-danger"
          @click="deleteBtn.clicked"
        >
          <i class="fas fa-trash" />
          {{ deleteBtn.text }}
        </button>
        <button
          :id="successBtn.id"
          :disabled="successBtn.disabled"
          type="button"
          class="ui-button btn btn-primary"
          @click="successBtn.clicked"
        >
          {{ successBtn.text }}
        </button>
        <button
          :id="cancelBtn.id"
          type="button"
          class="ui-button btn btn-success"
          @click="cancelBtn.clicked"
        >
          {{ cancelBtn.text }}
        </button>
      </template>
    </ModalWindow>
  </transition>
</template>

<script>
import ModalWindow from '../../components/ModalWindow.vue';
import ContractGrid from './ContractGrid.vue';
import {
  deepCopy,
  isNullOrWhiteSpace,
  last,
  findParent,
  findRowInTreeAndDelete
} from './dataHelpers';
import { updateProperties, getPropertiesCopy } from './contractParser';

export default {
  name: 'AddModelModalWindow',
  components: {
    ModalWindow,
    ContractGrid
  },
  props: {
    title: { type: String, default: '' },
    fieldName: { type: String, default: '' },
    parentName: { type: String, default: '' },
    modalRows: { type: Array, default: () => [] },
    editStack: { type: Array, default: () => [] }
  },
  data() {
    return {
      objectRows: [],
      modalFieldName: '',
      modalTitle: this.title,
      showFieldNameTextbox: false,
      isEditModal: false,
      isArray: false,
      successBtn: {
        id: 'saveModelBtn',
        text: 'Add Model',
        clicked: this.saveModel,
        disabled: this.isSaveDisabled
      },
      cancelBtn: {
        id: 'cancelBtn',
        text: 'Cancel',
        clicked: this.close
      },
      deleteBtn: {}
    };
  },
  computed: {
    isSaveDisabled() {
      return isNullOrWhiteSpace(this.modalFieldName) || this.modalRows.length === 0;
    },
    computedModalTitle: {
      get() {
        return this.modalTitle;
      },
      set(val) {
        this.modalTitle = val || this.title;
      }
    }
  },
  watch: {
    parentName(val) {
      this.modalFieldName = val;
    },
    /* eslint-disable object-shorthand */
    modalRows: function setDisabled(rows) {
      this.objectRows = deepCopy(rows);
      const isAnyObjectEmpty = rows.some(obj => {
        return (
          (obj.type === 'object' || (obj.items && obj.items.type === 'object')) &&
          getPropertiesCopy(obj).length === 0
        );
      });
      this.successBtn.disabled =
        isNullOrWhiteSpace(this.modalFieldName) || rows.length === 0 || isAnyObjectEmpty;
    },
    modalFieldName: function setDisabled(newFieldName) {
      this.successBtn.disabled = isNullOrWhiteSpace(newFieldName) || this.modalRows.length === 0;
    }
    /* eslint-enable object-shorthand */
  },
  methods: {
    enter() {
      this.modalFieldName = this.fieldName;
      this.successBtn = {
        id: 'saveModelBtn',
        text: 'Add Model',
        clicked: this.saveModel,
        disabled: this.isSaveDisabled
      };

      this.isArray = last(this.editStack).items !== undefined;

      this.showFieldNameTextbox = false;
      if (last(this.editStack).rowId !== undefined) {
        this.isEditModal = true;
        this.computedModalTitle = 'Update Model';
        this.successBtn = {
          id: 'saveModelBtn',
          text: 'Update Model',
          clicked: this.saveModel,
          disabled: this.isSaveDisabled
        };
        this.deleteBtn = {
          id: 'deleteModelBtn',
          text: 'Delete',
          clicked: this.deleteModel
        };
        this.showFieldNameTextbox = true;
      }
    },

    saveModel() {
      this.onCheckboxChange();
      const model = deepCopy(last(this.editStack));
      updateProperties(model, deepCopy(this.objectRows));
      this.$emit('save', model, this.modalFieldName);
    },

    deleteModel() {
      const model = deepCopy(last(this.editStack));
      if (this.editStack.length - 1 > 0) {
        this.editStack.pop();
        findRowInTreeAndDelete(this.editStack, model);
        const previousModel = last(this.editStack);
        this.modalFieldName = previousModel.name;
        this.$parent.currentIndex -= 1;
        const properties = getPropertiesCopy(previousModel);
        this.objectRows = deepCopy(properties);
        this.$parent.modalRows = deepCopy(properties);
      } else {
        this.$emit('delete', model);
      }
    },

    close() {
      if (this.editStack.length > 0) {
        this.editStack.pop();
        this.$emit('close', 'addModel', false, false);
      } else {
        this.$emit('close', 'addModel', false, true);
      }
    },

    showModelWindow(field) {
      const model = field;
      let parent = findParent(this.$parent.rows, field);
      if (parent === null) {
        parent = findParent(this.$parent.modalRows, field);
      }
      model.parentId = parent == null ? null : parent.rowId;
      this.editStack.push(deepCopy(model));
      this.objectRows = getPropertiesCopy(model);
      this.$parent.modalRows = deepCopy(this.objectRows);
      this.modalFieldName = model.name;
      this.$parent.parentName = model.name;
    },

    showFieldWindow(field) {
      let deepField = {};
      this.modalFieldName = this.parentName;
      if (field !== undefined) {
        deepField = deepCopy(field);
      }
      if (last(this.editStack).parentId === undefined) {
        // Parent is a new model that doesn't have an ID yet
        updateProperties(deepField, [], this.isArray);
      } else {
        // Parent is a model that has an ID which forms the parentId of the field
        deepField.parentId = last(this.editStack).rowId;
      }
      this.$emit('showFieldWindow', { ...deepField });
    },

    onCheckboxChange() {
      const model = last(this.editStack);
      model.type = this.isArray ? 'array' : 'object';
      updateProperties(model, undefined, this.isArray);
    }
  }
};
</script>
