<template>
  <ModalWindow
    ref="editManuallyModal"
    title="Edit Contract Manually"
    :success-btn="{
      id: 'updateContract',
      text: 'Update Field',
      clicked: updateData
    }"
    :cancel-btn="{
      id: 'cancelBtn',
      text: 'Cancel',
      clicked: close
    }"
  >
    <template v-slot:body>
      <textarea id="contract-raw" v-model="modifiedString" class="form-control tall mono" />
    </template>
  </ModalWindow>
</template>

<script>
import $ from 'jquery';
import ModalWindow from '../../components/ModalWindow.vue';

export default {
  name: 'EditContractModalWindow',
  components: {
    ModalWindow
  },
  props: {
    title: { type: String, default: '' },
    contractString: { type: String, default: '' }
  },
  data() {
    return {
      modifiedString: JSON.stringify(JSON.parse(this.contractString), null, 2)
    };
  },
  methods: {
    updateData() {
      this.$emit('updateData');
    },
    close() {
      $('#contract-raw')[0].value = JSON.stringify(
        JSON.parse($('#ModifiedContract_ContractString').val()),
        null,
        2
      );
      this.$emit('close');
      $('#contract-raw').scrollTop(0);
    }
  }
};
</script>
