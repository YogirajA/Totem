import $ from 'jquery';
import { deepCopy } from './dataHelpers';

let currentRowCount = 0;

export const formatReferenceName = referenceString => referenceString.substring(2);

/* convertPropertiesToArray: recursively iterates through a contract schema object and converts
   it into an array of rows. "properties" also becomes an array of child rows. */
export const convertPropertiesToArray = (obj, schema) => {
  const propertyArray = [];
  Object.keys(obj).forEach(property => {
    const item = obj[property];
    if (item.properties) {
      item.properties = convertPropertiesToArray(item.properties, schema);
    }
    if (item.items && item.items.properties) {
      item.items.properties = convertPropertiesToArray(item.items.properties, schema);
    }
    currentRowCount += 1;
    item.name = property;
    item.rowId = currentRowCount;
    if (!item.type) {
      let reference = item.$ref;
      if (reference) {
        reference = formatReferenceName(reference);
        const referenceObject = schema[reference];
        item.reference = reference;
        item.type = referenceObject.type;
        item.pattern = referenceObject.pattern;
        item.example = item.example || referenceObject.example;
      }
    }
    propertyArray.push(item);
  });
  return propertyArray;
};

/* getModalRows: Builds the rows to display in the table based on the parsed contract object.
   Sets initial row count to give the rows temporary IDs for editing/deleting; needs to increment
   continually through the recursion so that each property gets a unique ID */
export const getModalRows = (object, schema) => {
  currentRowCount = 0;
  return convertPropertiesToArray(object, schema);
};

/* parseContractArray: creates the "rows" array used by the root grid from a complete contract
   string, and displays validation errors from parsing (if any) */
export const parseContractArray = (contractString, validationFieldId) => {
  let schema;
  const $validationField = $(`#${validationFieldId}`);

  try {
    $validationField.html('');
    schema = JSON.parse(contractString);
  } catch (e) {
    $validationField.html(e);
    return null;
  }

  if (!schema.Contract) {
    $validationField.html('Contract must contain a "Contract" object.');
    return null;
  }

  const { properties } = schema.Contract;
  const propertyArray = getModalRows(properties, schema);

  return propertyArray;
};

/* findRow: searches a collection of rows and returns the row based on rowId */
export const findRow = (rowId, rows) => {
  let result;
  function searchForRow(a) {
    if (a.rowId === rowId) {
      result = a;
      return true;
    }

    const properties = (a.items && a.items.properties) || a.properties;

    return Array.isArray(properties) && properties.some(searchForRow);
  }

  rows.some(searchForRow);
  return result;
};

/* buildPropertiesFromMessage: build the properties for an object from the given message */
export const buildPropertiesFromMessage = (parentObject, messageObject) => {
  const updatedParentObject = deepCopy(parentObject);
  Object.keys(messageObject).forEach(key => {
    if (Array.isArray(messageObject[key])) {
      const prop = {
        type: 'array',
        example: '["sample string"]',
        items: {
          type: 'string',
          example: 'sample string'
        }
      };
      updatedParentObject.properties[key] = prop;
    } else if (messageObject[key] instanceof Object) {
      const prop = {
        type: 'object',
        properties: {}
      };
      updatedParentObject.properties[key] = prop;
      updatedParentObject.properties[key] = buildPropertiesFromMessage(
        updatedParentObject.properties[key],
        messageObject[key]
      );
    } else {
      const prop = {
        type: 'string',
        example: 'sample string'
      };
      updatedParentObject.properties[key] = prop;
    }
  });
  return updatedParentObject;
};

/* buildContractFromMessage: build a contract from the given message */
export const buildContractFromMessage = message => {
  const messageObject = JSON.parse(message);
  const baseContractObject = {
    Contract: {
      type: 'object',
      properties: {}
    }
  };
  baseContractObject.Contract = buildPropertiesFromMessage(
    baseContractObject.Contract,
    messageObject
  );
  return baseContractObject;
};

/* updateNestedProperty: finds the existing row by rowId and replaces it with the edited value */
export const updateNestedProperty = (editedRow, rows, isDelete) => {
  const updatedRows = deepCopy(rows);
  let rowDeleted = false;

  rows.forEach((row, index) => {
    if (
      (row.modalRowId !== undefined && row.modalRowId === editedRow.modalRowId) ||
      (row.rowId !== null && row.rowId === editedRow.rowId)
    ) {
      if (isDelete) {
        updatedRows.splice(index, 1);
        rowDeleted = true;
      } else {
        updatedRows[index] = editedRow;
      }
    } else if (row.properties !== undefined && !rowDeleted) {
      updatedRows[index].properties = updateNestedProperty(editedRow, row.properties, isDelete);
    } else if (row.items && row.items.properties !== undefined && !rowDeleted) {
      updatedRows[index].items.properties = updateNestedProperty(
        editedRow,
        row.items.properties,
        isDelete
      );
    }
  });
  return updatedRows;
};

/* buildNestedProperties: recursive loop used to turn nested property arrays back into
  objects, filtering out the fields we don't want in the final contractString */
export const buildNestedProperties = rows => {
  const object = {};
  rows.forEach(row => {
    object[row.name] = {};
    if (row.properties) {
      object[row.name].type = 'object';
      object[row.name].properties = buildNestedProperties(row.properties);
    } else if (row.items && row.items.properties) {
      object[row.name].type = 'array';
      object[row.name].items = {};
      object[row.name].items.type = 'object';
      object[row.name].items.properties = buildNestedProperties(row.items.properties);
    } else if (row.$ref) {
      object[row.name].$ref = row.$ref;
      object[row.name].example = row.example;
    } else {
      Object.keys(row).forEach(property => {
        if (
          property !== 'name' &&
          property !== 'rowId' &&
          property !== 'reference' &&
          property !== 'isLocked' &&
          property !== 'parentId' &&
          property !== 'modalRowId'
        ) {
          object[row.name][property] = row[property];
        }
      });
    }
  });
  return object;
};

/* createContractString: stringifies the data in "rows" array to become a valid contract string.
   Copies any existing schema objects besides Contract over to the new contract string. */
export const createContractString = (rows, schema) => {
  const newContractObject = {
    Contract: {
      type: 'object',
      properties: buildNestedProperties(rows)
    }
  };
  Object.keys(schema).forEach(model => {
    if (model !== 'Contract') {
      newContractObject[model] = schema[model];
    }
  });
  return JSON.stringify(newContractObject);
};

/* updateContractString: turns a new or edited row into a contractString; should be called after
   all edit windows have closed to create the new modifiedContract string */
export const updateContractString = (updatedRow, rows, contractString, isDelete = false) => {
  let updatedRows = deepCopy(rows);
  const schema = JSON.parse(contractString);
  if (updatedRow.rowId !== undefined) {
    // Update the existing row
    updatedRows = updateNestedProperty(updatedRow, rows, isDelete);
  } else if (updatedRow.parentId === null || updatedRow.parentId === undefined) {
    // Add the new row to the root object
    updatedRows.push(updatedRow);
  } else {
    // Traverse the object until the parent is found, and insert the row there
    updatedRows.forEach(row => {
      if (row.rowId === updatedRow.parentId) {
        updatedRows.properties.push(row);
      }
    });
  }
  return createContractString(updatedRows, schema);
};

/* getPropertiesCopy: gets properties copy from a schema object or aray of objects, otherwise empty array */
export const getPropertiesCopy = schema => {
  return deepCopy(schema.properties || (schema.items && schema.items.properties) || []);
};

/* hasProperties: return true or false if the schema object has properties */
export const hasProperties = schema => {
  return (schema.properties || (schema.items && schema.items.properties)) !== undefined;
};

/* isObjectArray: return true or false if the schema object is an array of objects */
export const isObjectArray = schema => {
  return (schema.items && schema.items.properties) !== undefined;
};

/* updateProperties: update the properties depending if the schema object type is an object or anrray of objects */
export const updateProperties = (schema, properties, isArray) => {
  /* eslint-disable */
  if (properties === undefined) {
    // eslint-disable-next-line no-param-reassign
    properties = getPropertiesCopy(schema);
  }
  if (isArray === undefined) {
    // eslint-disable-next-line no-param-reassign
    isArray = isObjectArray(schema);
  }
  schema.properties = isArray ? undefined : properties;
  schema.items = isArray ? { type: 'object', properties } : undefined;
  /* eslint-enable */
};

/* createSchemaString: creates a contract string for new models, that don't need the full "Contract" w/ references included */
export const createSchemaString = schema => {
  const isObjArray = isObjectArray(schema);
  const cleanSchema = {
    type: isObjArray ? 'array' : 'object'
  };
  const properties = buildNestedProperties(
    isObjArray ? schema.items.properties : schema.properties
  );
  cleanSchema.properties = isObjArray ? undefined : properties;
  cleanSchema.items = isObjArray ? { properties } : undefined;
  return JSON.stringify(cleanSchema);
};

/* buildNestedOptions: used for recursively adding options to the option array for dropdown in case of nested objects */
export const buildNestedOptions = (propertyArray, options) => {
  propertyArray.forEach(property => {
    if (property.type === 'object' || (property.items && property.items.type === 'object')) {
      options.push({
        id: options.length + 1,
        displayName: property.name,
        value: {
          id: options.length + 1,
          schemaName: property.name,
          schemaString: createSchemaString(property)
        },
        isObject: true
      });
      buildNestedOptions(
        (property.items && property.items.properties) || property.properties,
        options
      );
    }
  });
};

/* getExistingOptions: creates the "options" array for the add field dropdown with the existing objects found in the contract
   string */
export const getExistingOptions = contractString => {
  const optionArray = [];
  const schema = JSON.parse(contractString);

  const { properties } = schema.Contract;
  const propertyArray = convertPropertiesToArray(properties, schema);

  buildNestedOptions(propertyArray, optionArray);
  return optionArray;
};

/* buildNewObject: when a field is saved, create a new object with the relevant data to
   pass back up to its parent */
export const buildNewObject = (name, type, isArray, example, currentModel, modifiedContract) => {
  const newObject = { name };
  if (Number.isInteger(type.id)) {
    const schema = JSON.parse(type.value.schemaString);
    // This data type was created by the client; known data types from the database have Guid ID's
    const properties = getModalRows(getPropertiesCopy(schema), JSON.parse(modifiedContract));
    updateProperties(newObject, properties, isArray);
    newObject.type = isArray ? 'array' : 'object';
  } else {
    const properties = JSON.parse(type.value.schemaString);

    if (isArray === true) {
      newObject.type = 'array';
      newObject.example = null;
      newObject.items = {};

      if (type.displayName === 'Guid') {
        newObject.items.reference = 'Guid';
        newObject.items.$ref = '#/Guid';
      } else {
        Object.keys(properties).forEach(property => {
          newObject.items[property] = properties[property];
        });
      }
    } else {
      Object.keys(properties).forEach(property => {
        newObject[property] = properties[property];
      });

      if (type.displayName === 'Guid') {
        newObject.reference = 'Guid';
        newObject.$ref = '#/Guid';
      }
    }
  }
  if (example || example === '') {
    newObject.example = example;
  }
  if (currentModel && currentModel.parentId !== undefined) {
    newObject.parentId = currentModel.parentId;
  }
  if (currentModel && currentModel.rowId !== undefined) {
    newObject.rowId = currentModel.rowId;
  }
  if (currentModel && currentModel.modalRowId !== undefined) {
    newObject.modalRowId = currentModel.modalRowId;
  }
  if (!currentModel) {
    newObject.parentId = null;
  }
  return newObject;
};
