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

export const last = array => array[array.length - 1];
