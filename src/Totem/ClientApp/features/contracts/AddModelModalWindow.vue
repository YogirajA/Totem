<template>
  <transition @enter="enter">
    <ModalWindow
      class-name="scroll-overflow"
      :title="`${modalTitle + ':  ' + modalFieldName}`"
      :success-btn="successBtn"
      :cancel-btn="cancelBtn"
    >
      <template v-slot:body>
        <div class="form-group">
          <label for="modelName" class="control-label">Property Name</label>
          <input
            id="modelName"
            v-model="modalFieldName"
            class="form-control"
            placeholder="Property Name"
          />
        </div>
        <ContractGrid
          :rows="modalRows"
          :is-ellipsis-menu-visible="false"
          @hideEllipsisMenu="true"
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
        <button type="button" class="ui-button btn btn-success" @click="cancelBtn.clicked">
          {{ cancelBtn.text }}
        </button>
      </template>
    </ModalWindow>
  </transition>
</template>

<script>
import ModalWindow from '../../components/ModalWindow.vue';
import ContractGrid from './ContractGrid.vue';
import { deepCopy, isNullOrWhiteSpace } from './dataHelpers';

export default {
  name: 'AddModelModalWindow',
  components: {
    ModalWindow,
    ContractGrid
  },
  props: {
    title: { type: String, default: '' },
    fieldName: { type: String, default: '' },
    modalRows: { type: Array, default: () => [] },
    partialContract: { type: Array, default: () => [] },
    currentIndex: { type: Number, default: -1 }
  },
  data() {
    return {
      rows: [],
      modalTitle: this.title,
      modalFieldName: this.fieldName,
      showFieldNameTextbox: false,
      isEditModal: false,
      successBtn: {
        id: 'saveModelBtn',
        text: 'Add Model',
        clicked: this.saveModel,
        disabled: this.isSaveDisabled
      },
      cancelBtn: {
        text: 'Cancel',
        clicked: this.close
      },
      deleteBtn: {}
    };
  },
  computed: {
    isSaveDisabled() {
      return isNullOrWhiteSpace(this.modalFieldName) || this.modalRows.length === 0;
    }
  },
  watch: {
    /* eslint-disable object-shorthand */
    modalRows: function setDisabled(rows) {
      this.successBtn.disabled = isNullOrWhiteSpace(this.modalFieldName) || rows.length === 0;
    },
    modalFieldName: function setDisabled(newFieldName) {
      this.successBtn.disabled = isNullOrWhiteSpace(newFieldName) || this.modalRows.length === 0;
    }
    /* eslint-enable object-shorthand */
  },
  methods: {
    enter() {
      this.modalFieldName = this.fieldName;
      this.modalTitle = this.title;
      this.successBtn = {
        id: 'saveModelBtn',
        text: 'Add Model',
        clicked: this.saveModel,
        disabled: this.isSaveDisabled
      };
      this.showFieldNameTextbox = false;
      if (this.partialContract[this.currentIndex].rowId !== undefined) {
        this.isEditModal = true;
        this.modalTitle = 'Update Model';
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
      const model = deepCopy(this.partialContract[this.currentIndex]);
      this.$emit('save', model, this.modalFieldName);
    },

    deleteModel() {
      const model = deepCopy(this.partialContract[this.currentIndex]);
      this.$emit('delete', model);
    },

    close() {
      this.$emit('close', 'addModel', false, true);
    },

    showFieldWindow(field) {
      let deepField = {};
      if (field !== undefined) {
        deepField = deepCopy(field);
      }
      if (this.partialContract[this.currentIndex].parentId === undefined) {
        // Parent is a new model that doesn't have an ID yet
        deepField.properties = [];
      }
      this.$emit('showFieldWindow', { ...deepField });
    }
  }
};
</script>
