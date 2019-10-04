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
        return `array (${reference.toLowerCase()})`;
      }
      if (format) {
        return `array (${format})`;
      }
      return `array (${type})`;
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
    if (
      (row.rowId !== undefined && row.rowId === updatedModel.rowId) ||
      (row.modalRowId !== undefined && row.modalRowId === updatedModel.modalRowId)
    ) {
      const model = row;
      model.name = updatedModel.name;
      model.modalRowId = updatedModel.modalRowId;
      model.properties = updatedModel.properties;
      model.items = updatedModel.items;
      rowUpdated = true;
      return true;
    }
    return (
      (Array.isArray(row.properties) && row.properties.some(searchAndUpdateRow)) ||
      (row.items &&
        Array.isArray(row.items.properties) &&
        row.items.properties.some(searchAndUpdateRow))
    );
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
    if (
      row.items &&
      Array.isArray(row.items.properties) &&
      row.items.properties.some(prop => prop.rowId === rowToDelete.rowId)
    ) {
      // eslint-disable-next-line no-param-reassign
      row.items.properties = row.items.properties.filter(prop => {
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
  const childRowId = childRow.rowId || childRow.modalRowId;
  let parentRow = null;

  function containsChild(row) {
    if (
      (Array.isArray(row.properties) &&
        row.properties.some(prop => prop.rowId === childRowId || prop.modalRowId === childRowId)) ||
      (row.items &&
        Array.isArray(row.items.properties) &&
        row.items.properties.some(
          prop => prop.rowId === childRowId || prop.modalRowId === childRowId
        ))
    ) {
      parentRow = deepCopy(row);
    }
    return (
      (Array.isArray(row.properties) && row.properties.forEach(containsChild)) ||
      (row.items &&
        Array.isArray(row.items.properties) &&
        row.items.properties.forEach(containsChild))
    );
  }

  if (childRowId) {
    tree.forEach(containsChild);
  }

  return parentRow;
};

export const isValidJSON = msg => {
  try {
    JSON.parse(msg);
  } catch (e) {
    return false;
  }
  return true;
};

export const last = array => array[array.length - 1];
