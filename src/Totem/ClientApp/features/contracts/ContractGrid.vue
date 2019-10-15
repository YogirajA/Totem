<template>
  <div>
    <TreeGrid
      class="treegrid"
      :data="rows"
      :columns="columns"
      :menu="menu"
      @editRowClick="handleEditClick"
      @editManually="$emit('editManually')"
      @showFieldWindow="$emit('showFieldWindow')"
    >
      <template slot="editableTemplate" slot-scope="scope">
        <i v-if="scope.row.isLocked" class="fas edit fa-lock" />
        <i v-else class="fas edit fa-pencil-alt" />
      </template>
      <template slot="typeTemplate" slot-scope="scope">{{ getType(scope.row) }}</template>
    </TreeGrid>
  </div>
</template>

<script>
import TreeGrid from '../../components/TreeGrid.vue';
import EllipsisMenu from '../../components/EllipsisMenu.vue';
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
          template: 'editableTemplate',
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
          template: 'typeTemplate'
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
      if (row.type === 'object' || (row.items && row.items.type === 'object')) {
        this.$emit('showModelWindow', row, false);
      } else {
        this.$emit('showFieldWindow', row);
      }
    },
    showEditContractModal() {
      this.$emit('editManually');
    },
    showAddNewFieldModal() {
      this.$emit('showFieldWindow');
    },
    getType(property) {
      return getDisplayType(property);
    },
    menu(column, table) {
      const menu = {
        body: null,
        className: null
      };
      if (column.key === 'example') {
        menu.body = table.$slots.buttongroup ? (
          table.$slots.buttongroup
        ) : (
          /* eslint-disable */
          <div class="btn-group">
            <button
              id="addNewFieldBtn"
              class="ui-button btn grid-btn"
              onClick={this.showAddNewFieldModal}
              type="button"
            >
              <i class="fa fa-plus" />
              Add New Field
            </button>
            {/* eslint-enable */
            this.isEllipsisMenuVisible && (
              // eslint-disable-next-line react/no-string-refs
              <EllipsisMenu ref="ellipsisMenu" onShowModal={this.showEditContractModal} />
            )}
          </div>
        );
        menu.className = 'menu-header';
      }
      return menu;
    }
  }
};
</script>
