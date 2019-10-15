/* TreeGrid, GridHeader, and GridBody components simplified from https://github.com/MisterTaki/vue-table-with-tree-grid */

import EllipsisMenu from './EllipsisMenu.vue';

export default {
  name: 'GridHeader',
  components: {
    EllipsisMenu
  },
  props: {
    isEllipsisMenuVisible: {
      type: Boolean,
      default: false
    }
  },
  data: () => ({}),
  computed: {
    table() {
      return this.$parent;
    }
  },
  methods: {
    showEditContractModal() {
      this.$emit('editManually');
    },
    showAddNewFieldModal() {
      this.$emit('showFieldWindow');
    }
  },
  render() {
    function getClassName(type, { headerAlign, prop }, table) {
      if (type === 'cell' || type === 'inner') {
        const classList = [];
        if (type === 'cell') {
          classList.push('treegrid-header-cell');
          if (['center', 'right'].indexOf(headerAlign) > -1) {
            classList.push(`treegrid-${headerAlign}-cell`);
          }
        }
        if (type === 'inner') {
          classList.push('treegrid-cell-inner');
          if (table.treeType && table.treeProp === prop) {
            classList.push('treegrid-tree-header-inner');
          }
        }
        return classList.join(' ');
      }
      return null;
    }

    function renderLabel(column) {
      return column.label ? column.label : '';
    }

    // template
    return (
      <table cellSpacing="0" cellPadding="0" border="0" class="treegrid-header">
        <colgroup>
          {this.table.tableColumns.map(column => (
            <col width={column.computedWidth || column.minWidth || column.width} />
          ))}
        </colgroup>
        <thead>
          <tr class="treegrid-header-row">
            {this.table.tableColumns.map((column, columnIndex) => {
              let menu = null;
              let className = null;
              if (column.key === 'example') {
                menu = this.table.$slots.buttongroup ? (
                  this.table.$slots.buttongroup
                ) : (
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
                    {this.isEllipsisMenuVisible && (
                      // eslint-disable-next-line react/no-string-refs
                      <EllipsisMenu ref="ellipsisMenu" onShowModal={this.showEditContractModal} />
                    )}
                  </div>
                );
                className = 'menu-header';
              }
              return (
                <th class={`${getClassName.call(this, 'cell', column, this.table)} ${className}`}>
                  <div class={getClassName.call(this, 'inner', column, this.table)}>
                    <div>{renderLabel.call(this, column, columnIndex)}</div>
                    {menu}
                  </div>
                </th>
              );
            })}
          </tr>
        </thead>
      </table>
    );
  }
};
