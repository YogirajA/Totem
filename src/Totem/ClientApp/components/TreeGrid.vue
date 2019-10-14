<!-- TreeGrid, GridHeader, and GridBody components simplified from https://github.com/MisterTaki/vue-table-with-tree-grid -->

<template>
  <div v-if="columns.length > 0">
    <div class="treegrid-header-wrapper">
      <GridHeader :add-menu="menu" />
    </div>
    <div class="treegrid-body-wrapper">
      <GridBody />
    </div>
  </div>
</template>

<script>
import GridHeader from './GridHeader';
import GridBody from './GridBody';

function getBodyData(data, isTreeType, childrenProp, arrayItemsProp, isFold, level = 1) {
  let bodyData = [];
  data.forEach((row, index) => {
    const arrayItems = row[arrayItemsProp];
    const arrayChildren = arrayItems ? arrayItems[childrenProp] : undefined;
    const children = arrayChildren || row[childrenProp];
    const childrenLen =
      Object.prototype.toString.call(children).slice(8, -1) === 'Array' ? children.length : 0;
    bodyData.push({
      level,
      isHide: isFold ? level !== 1 : false,
      isFold,
      childrenLen,
      normalIndex: index + 1,
      ...row
    });
    if (isTreeType) {
      if (childrenLen > 0) {
        bodyData = bodyData.concat(
          getBodyData(children, true, childrenProp, arrayItemsProp, isFold, level + 1)
        );
      }
    }
  });
  return bodyData;
}

function initialState(table) {
  return {
    bodyHeight: 'auto',
    treeProp: 'name',
    bodyData: getBodyData(
      table.data,
      table.treeType,
      table.childrenProp,
      table.arrayItemsProp,
      table.isFold
    )
  };
}

function initialColumns(table, clientWidth) {
  let columnsWidth = 0;
  const minWidthColumns = [];
  const otherColumns = [];
  const columns = table.columns.concat();

  columns.forEach((column, index) => {
    let width = '';
    let minWidth = '';
    if (!column.width) {
      if (column.minWidth) {
        minWidth =
          typeof column.minWidth === 'number' ? column.minWidth : parseInt(column.minWidth, 10);
      } else {
        minWidth = 80;
      }
      minWidthColumns.push({
        ...column,
        minWidth,
        index
      });
    } else {
      width = typeof column.width === 'number' ? column.width : parseInt(column.width, 10);
      otherColumns.push({
        ...column,
        width,
        index
      });
    }
    columnsWidth += minWidth || width;
  });
  const scrollBarWidth = 16;
  const totalWidth = columnsWidth + scrollBarWidth;
  const isScrollX = totalWidth > clientWidth;
  if (!isScrollX) {
    const extraWidth = clientWidth - totalWidth;
    const averageExtraWidth = Math.floor(extraWidth / minWidthColumns.length);
    minWidthColumns.forEach(column => {
      const updatedColumn = column;
      updatedColumn.computedWidth = column.minWidth + averageExtraWidth;
      return updatedColumn;
    });
  }
  const tableColumns = otherColumns.concat(minWidthColumns);
  tableColumns.sort((a, b) => a.index - b.index);
  return tableColumns;
}

export default {
  name: 'TreeGrid',
  components: {
    GridHeader,
    GridBody
  },
  props: {
    data: {
      type: Array,
      default: () => []
    },
    columns: {
      type: Array,
      default: () => []
    },
    maxHeight: {
      type: [String, Number],
      default: 'auto'
    },
    treeType: {
      type: Boolean,
      default: true
    },
    childrenProp: {
      type: String,
      default: 'properties'
    },
    arrayItemsProp: {
      type: String,
      default: 'items'
    },
    isFold: {
      type: Boolean,
      default: false
    },
    emptyText: {
      type: String,
      default: 'None'
    },
    menu: Function,
    rowKey: Function,
    rowClassName: [String, Function],
    cellClassName: [String, Function],
    rowStyle: [Object, Function],
    cellStyle: [Object, Function]
  },
  data() {
    return {
      computedWidth: '',
      computedHeight: '',
      tableColumns: [],
      ...initialState(this)
    };
  },
  computed: {
    bodyWrapperStyle() {
      return {
        height: this.bodyHeight
      };
    },
    tableClass() {
      return {};
    },
    bodyClass() {
      return {};
    }
  },
  watch: {
    $props: {
      deep: true,
      handler() {
        Object.assign(this.$data, initialState(this));
      }
    }
  },
  updated() {
    this.measure();
  },
  mounted() {
    this.measure();
    window.addEventListener('resize', this.measure);
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.measure);
  },
  methods: {
    handleEvent(type, $event) {
      const eventType = $event.type;
      return this.$emit(`${type}-${eventType}`, $event);
    },
    measure() {
      this.$nextTick(() => {
        const { clientWidth, clientHeight } = this.$el;
        const width = clientWidth === 0 ? 800 : clientWidth;
        this.computedWidth = width + 2;
        this.computedHeight = clientHeight + 2;

        const maxHeight = parseInt(this.maxHeight, 10);
        if (this.maxHeight !== 'auto' && this.computedHeight > maxHeight) {
          this.bodyHeight = `${maxHeight - 83}px`;
        }
        this.tableColumns = initialColumns(this, width);
      });
    }
  }
};
</script>
