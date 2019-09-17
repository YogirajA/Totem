<template>
  <div class="modal-backdrop">
    <div :class="modalClassName">
      <header class="modal-header">
        <slot name="header">
          {{ title }}
        </slot>
      </header>
      <section class="modal-body">
        <slot name="body">
          Test
        </slot>
      </section>
      <footer class="modal-footer">
        <slot name="footer">
          <button
            v-show="isEditing"
            :id="deleteButton.id"
            type="button"
            class="ui-button btn btn-danger"
            :disabled="disableDelete"
            @click="deleteButton.clicked"
          >
            <i class="fas fa-trash" />{{ deleteBtn ? deleteBtn.text : '' }}
          </button>
          <button
            :id="successButton.id"
            :disabled="successBtn.disabled"
            type="button"
            class="ui-button btn btn-primary"
            @click="successButton.clicked"
          >
            {{ successButton.text }}
          </button>
          <button
            :id="cancelButton.id"
            type="button"
            class="ui-button btn btn-success"
            @click="cancelButton.clicked"
          >
            {{ cancelButton.text }}
          </button>
        </slot>
      </footer>
    </div>
  </div>
</template>

<script>
export default {
  name: 'ModalWindow',
  props: {
    title: { type: String, default: '' },
    successBtn: { type: Object, default: () => {} },
    cancelBtn: { type: Object, default: () => {} },
    deleteBtn: { type: Object, default: () => {} },
    className: { type: String, default: '' },
    isEditing: { type: Boolean, default: false },
    disableDelete: { type: Boolean, default: false }
  },
  computed: {
    modalClassName() {
      return `modal ${this.className || ''}`;
    },
    successButton() {
      return {
        id: this.successBtn.id || 'modalSaveButton',
        text: this.successBtn.text || 'Ok',
        clicked:
          this.successBtn.clicked ||
          function btnClicked() {
            this.$emit('save');
          }
      };
    },
    deleteButton() {
      return {
        id: this.deleteBtn ? this.deleteBtn.id : 'modalDeleteButton',
        text: this.deleteBtn ? this.deleteBtn.text : 'Delete',
        clicked: this.deleteBtn
          ? this.deleteBtn.clicked
          : function btnClicked() {
              this.$emit('delete');
            }
      };
    },
    cancelButton() {
      return {
        id: this.cancelBtn.id || 'modalCancelButton',
        text: this.cancelBtn.text || 'Cancel',
        clicked:
          this.cancelBtn.clicked ||
          function btnClicked() {
            this.$emit('close');
          }
      };
    }
  }
};
</script>
