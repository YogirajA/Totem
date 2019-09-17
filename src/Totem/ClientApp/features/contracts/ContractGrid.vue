<template>
  <div>
    <TreeGrid
      class="treegrid"
      :data="rows"
      :columns="columns"
      :is-ellipsis-menu-visible="isEllipsisMenuVisible"
      @editClick="handleEditClick"
      @editManually="$emit('editManually')"
      @showFieldWindow="$emit('showFieldWindow')"
    >
      <template slot="editable" slot-scope="scope">
        <i v-if="scope.row.isLocked" class="fas edit fa-lock" />
        <i v-else class="fas edit fa-pencil-alt" />
      </template>
      <template slot="type" slot-scope="scope">
        {{ getType(scope.row) }}
      </template>
    </TreeGrid>
  </div>
</template>

<script>
import TreeGrid from '../../components/TreeGrid.vue';
import { getDisplayType } from './dataHelpers';

export default {
  name: 'ContractGrid',
  components: {
    TreeGrid
  },
  props: {
    rows: { type: Array, default: () => [] },
    isEllipsisMenuVisible: {
      type: Boolean,
      default: true
    }
  },
  data() {
    return {
      columns: [
        {
          key: 'edit',
          width: '50px',
          type: 'template',
          template: 'editable',
          align: 'center'
        },
        {
          label: 'Property',
          key: 'name'
        },
        {
          label: 'Data Model',
          width: '150px',
          key: 'type',
          type: 'template',
          template: 'type'
        },
        {
          label: 'Example',
          key: 'example'
        }
      ]
    };
  },
  methods: {
    handleEditClick(row) {
      if (row.type === 'object') {
        this.$emit('showModelWindow', row, false);
      } else {
        this.$emit('showFieldWindow', row);
      }
    },
    getType(property) {
      return getDisplayType(property);
    }
  }
};
</script>
