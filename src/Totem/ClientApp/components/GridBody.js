/* TreeGrid, GridHeader, and GridBody components simplified from https://github.com/MisterTaki/vue-table-with-tree-grid */

export default {
  name: 'GridBody',
  data: () => ({}),
  computed: {
    table() {
      return this.$parent;
    }
  },
  methods: {
    toggleExpand(type, row, rowIndex, value) {
      const target = this.table.bodyData[rowIndex];
      this.table.bodyData.splice(rowIndex, 1, {
        ...target,
        [`_is${type}`]: typeof value === 'undefined' ? !row[`_is${type}`] : value
      });
    },
    getChildrenIndex(parentLevel, parentIndex, careFold = true) {
      const data = this.table.bodyData;
      let childrenIndex = [];
      for (let i = parentIndex + 1; i < data.length; i += 1) {
        if (data[i].level <= parentLevel) break;
        if (data[i].level - 1 === parentLevel) {
          childrenIndex.push(i);
        }
      }
      const len = childrenIndex.length; // important!!!
      if (len > 0) {
        for (let i = 0; i < len; i += 1) {
          const childData = data[childrenIndex[i]];
          if (childData.childrenLen && (!careFold || (careFold && !childData.isFold))) {
            childrenIndex = childrenIndex.concat(
              this.getChildrenIndex(childData.level, childrenIndex[i], careFold)
            );
          }
        }
      }
      return childrenIndex;
    },
    handleEvent($event, type, data) {
      const eventType = $event ? $event.type : '';
      const { row, rowIndex, column, columnIndex } = data;
      const latestData = this.table.bodyData;

      if (type === 'icon') {
        $event.stopPropagation();
        this.toggleExpand('Fold', row, rowIndex);
        const childrenIndex = this.getChildrenIndex(row.level, rowIndex);
        for (let i = 0; i < childrenIndex.length; i += 1) {
          this.toggleExpand('Hide', latestData[childrenIndex[i]], childrenIndex[i]);
        }
        return this.table.$emit(
          'tree-icon-click',
          latestData[rowIndex],
          column,
          columnIndex,
          $event
        );
      }
      if (type === 'cell' && eventType === 'click' && column.key === 'edit' && !row.isLocked) {
        this.table.$emit('editClick', row, column, columnIndex, $event);
      }
      if (type === 'cell') {
        return this.table.$emit(
          `${type}-${eventType}`,
          latestData[rowIndex],
          rowIndex,
          column,
          columnIndex,
          $event
        );
      }
      return this.table.$emit(`${type}-${eventType}`, latestData[rowIndex], rowIndex, $event);
    }
  },
  render() {
    function getKey(row, rowIndex) {
      const { rowKey } = this.table;
      if (rowKey) {
        return rowKey.call(null, row, rowIndex);
      }
      return rowIndex;
    }

    function getStyle(type, row, rowIndex, column, columnIndex) {
      const style = this.table[`${type}Style`];
      if (typeof style === 'function') {
        if (type === 'row') {
          return style.call(null, row, rowIndex);
        }
        if (type === 'cell') {
          return style.call(null, row, rowIndex, column, columnIndex);
        }
      }
      return style;
    }

    function getClassName(type, row, rowIndex, column, columnIndex) {
      const classList = [];
      if (type === 'row' || type === 'cell') {
        const className = this.table[`${type}ClassName`];
        if (typeof className === 'string') {
          classList.push(className);
        } else if (typeof className === 'function') {
          if (type === 'row') {
            classList.push(className.call(null, row, rowIndex) || '');
          }
          if (type === 'cell') {
            classList.push(className.call(null, row, rowIndex, column, columnIndex) || '');
          }
        }
        if (type === 'row') {
          classList.push('treegrid-body-row');
          if (row && row.isLocked) {
            classList.push('locked');
          }
        }
        if (type === 'cell') {
          classList.push('treegrid-body-cell');
          const { align } = column;
          if (['center', 'right'].indexOf(align) > -1) {
            classList.push(`treegrid-${align}-cell`);
          }
        }
      }
      if (type === 'inner') {
        classList.push('treegrid-cell-inner');
      }
      if (type === 'inner' && column.key === 'edit' && !row.isLocked) {
        classList.push('edit-action');
      }
      return classList.join(' ');
    }

    function renderCell(row, rowIndex, column, columnIndex) {
      if (this.table.treeProp === column.key) {
        return (
          <span
            class={`treegrid-level-${row.level}-cell`}
            style={{
              marginLeft: `${(row.level - 1) * 24}px`,
              paddingLeft: row.childrenLen === 0 ? '17px' : ''
            }}
          >
            {row.childrenLen > 0 && (
              <i
                class={`far ${row.isFold ? 'fa-plus-square' : 'fa-minus-square'}`}
                on-click={$event =>
                  this.handleEvent(
                    $event,
                    'icon',
                    { row, rowIndex, column, columnIndex },
                    { isFold: row.isFold }
                  )
                }
              />
            )}
            {row[column.key] ? row[column.key] : ''}
          </span>
        );
      }
      if (column.type === undefined || column.type === 'custom') {
        return row[column.key];
      }
      if (column.type === 'template') {
        return this.table.$scopedSlots[column.template]
          ? this.table.$scopedSlots[column.template]({ row, rowIndex, column, columnIndex })
          : '';
      }
      return '';
    }

    // template
    return (
      <table cellSpacing="0" cellPadding="0" border="0" className="treegrid-body">
        <colgroup>
          {this.table.tableColumns.map(column => (
            <col width={column.computedWidth || column.minWidth || column.width} />
          ))}
        </colgroup>
        <tbody>
          {this.table.bodyData.length > 0 ? (
            this.table.bodyData.map((row, rowIndex) => [
              <tr
                v-show={!row.isHide}
                key={this.table.rowKey ? getKey(row, rowIndex) : rowIndex}
                style={getStyle.call(this, 'row', row, rowIndex)}
                class={getClassName.call(this, 'row', row, rowIndex)}
                on-click={$event => this.handleEvent($event, 'row', { row, rowIndex })}
              >
                {this.table.tableColumns.map((column, columnIndex) => (
                  <td
                    style={getStyle.call(this, 'cell', row, rowIndex, column, columnIndex)}
                    class={getClassName.call(this, 'cell', row, rowIndex, column, columnIndex)}
                    on-click={$event =>
                      this.handleEvent($event, 'cell', { row, rowIndex, column, columnIndex })
                    }
                  >
                    <div
                      class={getClassName.call(this, 'inner', row, rowIndex, column, columnIndex)}
                    >
                      {renderCell.call(this, row, rowIndex, column, columnIndex)}
                    </div>
                  </td>
                ))}
              </tr>
            ])
          ) : (
            <tr class="treegrid-empty-row">
              <td
                class="treegrid-body-cell treegrid-empty-content"
                colSpan={this.table.tableColumns.length}
              >
                {this.table.emptyText}
              </td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }
};
