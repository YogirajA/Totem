import _ from 'lodash';

export const reorderOptions = oldOptions => {
  // pull out the "Define new" option if it exists, and order the rest alphabetically
  const newOptions = _.sortBy(_.filter(oldOptions, option => option.id !== 0), 'displayName');
  // push the Define New object to the end
  newOptions.push({
    id: 0,
    displayName: 'Define a new model',
    value: null
  });
  return newOptions;
};

export const getDisplayType = property => {
  if (property.reference) {
    return property.reference.toLowerCase();
  }
  if (property.format) {
    return `${property.type} (${property.format})`;
  }
  if (property.type) {
    if (property.type === 'array') {
      const { reference, format, type } = property.items;
      if (reference) {
        return `array(${reference.toLowerCase()})`;
      }
      if (format) {
        return `array(${format})`;
      }
      return `array(${type})`;
    }
    return property.type;
  }
  return 'object';
};

export const deepCopy = object => JSON.parse(JSON.stringify(object));

export const isNullOrWhiteSpace = str => str === null || str.trim() === '';

export const getUniqueId = () => {
  return `_${Math.random()
    .toString(36)
    .substr(2, 9)}`;
};

export const findRowInTreeAndUpdate = (tree, updatedModel) => {
  const updatedTree = deepCopy(tree);
  let rowUpdated = false;

  function searchAndUpdateRow(row) {
    if (row.rowId === updatedModel.rowId) {
      row.name = updatedModel.name;
      row.modalRowId = updatedModel.modalRowId;
      row.properties = updatedModel.properties;
      rowUpdated = true;
      return true;
    }
    return Array.isArray(row.properties) && row.properties.some(searchAndUpdateRow);
  }

  updatedTree.forEach(searchAndUpdateRow);
  return rowUpdated ? updatedTree : undefined;
};

export const findRowInTreeAndDelete = (tree, rowToDelete) => {
  let rowDeleted = false;

  function searchAndDelete(row) {
    if (
      Array.isArray(row.properties) &&
      row.properties.some(prop => prop.rowId === rowToDelete.rowId)
    ) {
      // eslint-disable-next-line no-param-reassign
      row.properties = row.properties.filter(prop => {
        return prop.rowId !== rowToDelete.rowId;
      });
      rowDeleted = true;
      return true;
    }
    return Array.isArray(row.properties) && row.properties.forEach(searchAndDelete);
  }

  tree.forEach(searchAndDelete);
  return rowDeleted ? tree : undefined;
};

export const findParent = (tree, childRow) => {
  const childRowId = childRow.rowId;
  let parentRow = null;

  function containsChild(row) {
    if (Array.isArray(row.properties) && row.properties.some(prop => prop.rowId === childRowId)) {
      parentRow = deepCopy(row);
    }
    return Array.isArray(row.properties) && row.properties.forEach(containsChild);
  }
  tree.forEach(containsChild);
  return parentRow;
};

export const last = array => array[array.length - 1];
