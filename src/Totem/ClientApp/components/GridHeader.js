/* TreeGrid, GridHeader, and GridBody components simplified from https://github.com/MisterTaki/vue-table-with-tree-grid */
export default {
  name: 'GridHeader',
  props: {
    addMenu: Function
  },
  data: () => ({}),
  computed: {
    table() {
      return this.$parent;
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
              const menu = this.addMenu(column, this.table);
              return (
                <th
                  class={`${getClassName.call(this, 'cell', column, this.table)} ${
                    menu.className !== null ? menu.className : ''
                  }`}
                >
                  <div class={getClassName.call(this, 'inner', column, this.table)}>
                    <div>{renderLabel.call(this, column, columnIndex)}</div>
                    {menu.body !== null ? menu.body : ''}
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
