<template>
  <div class="dropdown ellipsis-menu">
    <a class="dropdown-toggle" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
      <i class="fas fa-ellipsis-h" />
    </a>
    <div class="dropdown-menu dropdown-menu-right">
      <button class="dropdown-item edit-manually" type="button" @click="showModal">
        Edit Contract Manually
      </button>
    </div>
  </div>
</template>

<script>
export default {
  name: 'EllipsisMenu',
  methods: {
    showModal() {
      this.$emit('showModal');
    }
  }
};
</script>
