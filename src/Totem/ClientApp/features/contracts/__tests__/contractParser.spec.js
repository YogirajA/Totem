import {
  parseContractArray,
  formatReferenceName,
  updateContractString,
  createSchemaString,
  buildNewObject,
  findRow,
  getExistingOptions,
  buildNestedOptions,
  getPropertiesCopy,
  hasProperties,
  isObjectArray,
  updateProperties
} from '../contractParser';

const sampleContractString = `{
  "Contract": {
    "type": "object",
    "properties": {
      "Id": {"$ref":"#/Guid"},
      "Timestamp": {
        "type": "string",
        "format": "date-time",
        "example": "2019-01-01T18:14:29Z"
      },
      "Address": {
        "type": "object",
        "properties": {
          "FullName": {
            "type": "object",
            "properties": {
              "FirstName": {
                "type": "string",
                "example": "John"
              },
              "LastName": {
                "type": "string",
                "example": "Doe"
              }
            }
          },
          "Street": {
            "name":"Street",
            "type":"string",
            "example":"123 Main St."
          }
        }
      }
    }
  },
  "Guid": {
    "type": "string",
    "pattern": "^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$",
    "minLength": 36,
    "maxLength": 36,
    "example": "01234567-abcd-0123-abcd-0123456789ab"
  }
}`;

describe('parseContractArray', () => {
  it('should generate an array of contract properties based on a contract ', () => {
    const contractString = `{
        "Contract": {
          "type": "object",
          "properties": {
            "Name": {
              "type": "string",
              "pattern": ".*"
            },
            "Age": { "type": "integer" }
          }
        }
      }`;

    const result = parseContractArray(contractString);

    expect(Array.isArray(result)).toBe(true);
    expect(result.length).toBe(2);
    expect(result[0].name).toEqual('Name');
    expect(result[0].type).toEqual('string');
    expect(result[0].pattern).toEqual('.*');
    expect(result[1].name).toEqual('Age');
    expect(result[1].type).toEqual('integer');
  });

  it('should handle nested objects, guids, and date-times, and required properties', () => {
    const result = parseContractArray(sampleContractString);

    expect(Array.isArray(result)).toBe(true);
    expect(result.length).toBe(3);
    expect(result[0].name).toEqual('Id');
    expect(result[0].type).toEqual('string');
    expect(result[0].reference).toEqual('Guid');
    expect(result[0].isLocked).toEqual(undefined);
    expect(result[1].name).toEqual('Timestamp');
    expect(result[1].type).toEqual('string');
    expect(result[1].format).toEqual('date-time');
    expect(result[1].isLocked).toEqual(undefined);
    expect(result[2].name).toEqual('Address');
    expect(result[2].type).toEqual('object');
    expect(result[2].isLocked).toEqual(undefined);
    expect(Array.isArray(result[2].properties)).toBe(true);
    expect(result[2].properties[0].name).toEqual('FullName');
    expect(result[2].properties[1].name).toEqual('Street');
    expect(result[2].properties[0].type).toEqual('object');
    expect(Array.isArray(result[2].properties[0].properties)).toBe(true);
    expect(result[2].properties[0].properties[0].name).toEqual('FirstName');
    expect(result[2].properties[0].properties[1].name).toEqual('LastName');
  });

  it('should populate contract properties for references (not nested)', () => {
    const contractString = `{
        "Contract": {
          "type": "object",
          "properties": {
            "Name": { "type": "string" },
            "Timestamp": { "type": "string", "format": "date-time" },
            "ID": { "$ref": "#/Guid" }
          }
        },
        "Guid": {
          "type": "string",
          "pattern": ".*"
        }
      }`;

    const result = parseContractArray(contractString);

    expect(Array.isArray(result)).toBe(true);
    expect(result.length).toBe(3);
    expect(result[2].name).toEqual('ID');
    expect(result[2].type).toEqual('string');
    expect(result[2].pattern).toEqual('.*');
    expect(result[2].reference).toEqual('Guid');
  });

  it('should return null if the JSON is invalid', () => {
    const contractString = `{
        "Contract": {
          "type": "object
      }`;

    const result = parseContractArray(contractString);

    expect(result).toBe(null);
  });

  it('should return null if the Contract object is missing the Contract field', () => {
    const contractString = `{
        "Guid": {
          "type": "string",
          "pattern": ".*"
        }
      }`;

    const result = parseContractArray(contractString);

    expect(result).toBe(null);
  });
});

describe('getExistingOptions', () => {
  it('should generate an array of options based on a contract ', () => {
    const contractString = `{
        "Contract": {
          "type": "object",
          "properties": {
            "Name": {
              "type": "string",
              "pattern": ".*"
            },
            "FullName": {
              "type": "object",
              "properties": {
                "FirstName": {
                  "type": "string",
                  "example": "John"
                },
                "LastName": {
                  "type": "string",
                  "example": "Doe"
                }
              }
            }
          }
        }
      }`;

    const result = getExistingOptions(contractString);

    expect(Array.isArray(result)).toBe(true);
    expect(result.length).toBe(1);
    expect(result[0].displayName).toEqual('FullName');
  });

  it('should handle nested objects and add them as options to the array', () => {
    const result = getExistingOptions(sampleContractString);

    expect(Array.isArray(result)).toBe(true);
    expect(result.length).toBe(2);
    expect(result[0].displayName).toEqual('Address');
    expect(result[1].displayName).toEqual('FullName');
  });
});

describe('formatReferenceName', () => {
  it('should format the reference name to remove preceding characters', () => {
    const referenceString = '#/FullName';
    const result = formatReferenceName(referenceString);
    expect(result).toEqual('FullName');
  });
});

describe('updateContractString', () => {
  let rows = [];
  beforeEach(() => {
    rows = [
      { name: 'Id', rowId: 1, $ref: '#/Guid' },
      { name: 'Timestamp', rowId: 2, type: 'string', format: 'date-time' },
      {
        name: 'Address',
        rowId: 3,
        type: 'object',
        properties: [
          {
            name: 'FullName',
            rowId: 4,
            type: 'object',
            properties: [
              { name: 'FirstName', rowId: 6, type: 'string' },
              { name: 'LastName', rowId: 7, type: 'string' }
            ]
          },
          { name: 'Street', rowId: 5, type: 'string' }
        ]
      }
    ];
  });

  it('adds a new field into the root of contractString', () => {
    const newRow = {
      name: 'NewRow',
      type: 'string',
      example: 'New string data'
    };
    const result = updateContractString(newRow, rows, sampleContractString);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.Id.$ref).toEqual('#/Guid');
    expect(objectResult.Contract.properties.Timestamp.format).toEqual('date-time');
    expect(objectResult.Contract.properties.NewRow.type).toEqual('string');
    expect(objectResult.Contract.properties.NewRow.example).toEqual('New string data');
  });

  it('updates an existing model at the root of contractString', () => {
    const editedAddress = {
      name: 'Address',
      rowId: 3,
      properties: [{ name: 'FirstOne', type: 'string' }, { name: 'SecondOne', type: 'string' }]
    };
    const result = updateContractString(editedAddress, rows, sampleContractString);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.Address.type).toEqual('object');
    expect(objectResult.Contract.properties.Address.properties.FirstOne.type).toEqual('string');
    expect(objectResult.Contract.properties.Address.properties.FullName).toEqual(undefined);
  });

  it('updates the contractString when an individual nested field is edited', () => {
    const editedRow = {
      name: 'NewName',
      type: 'integer',
      example: 123,
      rowId: 6
    };
    const result = updateContractString(editedRow, rows, sampleContractString);
    const objectResult = JSON.parse(result);
    expect(
      objectResult.Contract.properties.Address.properties.FullName.properties.NewName.type
    ).toEqual('integer');
    expect(
      objectResult.Contract.properties.Address.properties.FullName.properties.NewName.example
    ).toEqual(123);
    expect(
      objectResult.Contract.properties.Address.properties.FullName.properties.FirstName
    ).toEqual(undefined);
  });

  it('updates the contractString when any individual field is edited at one level nested', () => {
    const editedRow = {
      name: 'StreetAddress',
      rowId: 5,
      type: 'integer',
      example: 123
    };
    const result = updateContractString(editedRow, rows, sampleContractString);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.Address.type).toEqual('object');
    expect(objectResult.Contract.properties.Address.properties.StreetAddress.type).toEqual(
      'integer'
    );
    expect(objectResult.Contract.properties.Address.properties.StreetAddress.example).toEqual(123);
    expect(objectResult.Contract.properties.Address.properties.Street).toEqual(undefined);
  });

  it('updates the contractString when the name of a model has changed', () => {
    const editedAddress = {
      name: 'UserAddress',
      rowId: 3,
      type: 'object',
      properties: [{ name: 'FirstOne', type: 'string' }, { name: 'SecondOne', type: 'string' }]
    };
    const result = updateContractString(editedAddress, rows, sampleContractString);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.UserAddress.type).toEqual('object');
    expect(objectResult.Contract.properties.UserAddress.properties.FirstOne.type).toEqual('string');
    expect(objectResult.Contract.properties.Address).toEqual(undefined);
  });

  it('adds a new model at the root', () => {
    const newModel = {
      name: 'NewNested',
      type: 'object',
      properties: [{ name: 'FirstOne', type: 'string' }, { name: 'SecondOne', type: 'string' }]
    };
    const result = updateContractString(newModel, rows, sampleContractString);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.NewNested.type).toEqual('object');
    expect(objectResult.Contract.properties.NewNested.properties.FirstOne.type).toEqual('string');
    expect(objectResult.Contract.properties.NewNested.properties.FullName).toEqual(undefined);
  });

  it("removes the UI properties that shouldn't be part of a contract string", () => {
    const newModel = {
      name: 'NewFancyField',
      type: 'string',
      rowId: 1,
      isLocked: true,
      parentId: 2,
      reference: 'something'
    };
    const result = updateContractString(newModel, rows, sampleContractString);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.NewFancyField.type).toEqual('string');
    expect(objectResult.Contract.properties.NewFancyField.name).toEqual(undefined);
    expect(objectResult.Contract.properties.NewFancyField.rowId).toEqual(undefined);
    expect(objectResult.Contract.properties.NewFancyField.isLocked).toEqual(undefined);
    expect(objectResult.Contract.properties.NewFancyField.parentId).toEqual(undefined);
    expect(objectResult.Contract.properties.NewFancyField.reference).toEqual(undefined);
  });

  it('Deletes a field row', () => {
    const model = { name: 'Id', rowId: 1, $ref: '#/Guid' };
    const result = updateContractString(model, rows, sampleContractString, true);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.Id).toBe(undefined);
    expect(objectResult.Contract.properties.Timestamp.type).toBe('string');
  });

  it('Deletes a model row', () => {
    const model = {
      name: 'Address',
      rowId: 3,
      type: 'object',
      properties: [
        {
          name: 'FullName',
          rowId: 4,
          type: 'object',
          properties: [
            { name: 'FirstName', rowId: 6, type: 'string' },
            { name: 'LastName', rowId: 7, type: 'string' }
          ]
        },
        { name: 'Street', rowId: 5, type: 'string' }
      ]
    };
    const result = updateContractString(model, rows, sampleContractString, true);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.Address).toBe(undefined);
    expect(objectResult.Contract.properties.Id.$ref).toBe('#/Guid');
  });

  it('Deletes a nested field row', () => {
    const model = { name: 'FirstName', rowId: 6, type: 'string' };
    const result = updateContractString(model, rows, sampleContractString, true);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.Address.properties.FullName.properties.FirstName).toBe(
      undefined
    );
    expect(
      objectResult.Contract.properties.Address.properties.FullName.properties.LastName.type
    ).toBe('string');
  });

  it('Deletes a nested model row', () => {
    const model = {
      name: 'FullName',
      rowId: 4,
      type: 'object',
      properties: [
        { name: 'FirstName', rowId: 6, type: 'string' },
        { name: 'LastName', rowId: 7, type: 'string' }
      ]
    };
    const result = updateContractString(model, rows, sampleContractString, true);
    const objectResult = JSON.parse(result);
    expect(objectResult.Contract.properties.Address.properties.FullName).toBe(undefined);
    expect(objectResult.Contract.properties.Address.properties.Street.type).toBe('string');
  });
});

describe('createSchemaString', () => {
  it('creates a valid schema definition for a model', () => {
    const model = {
      name: 'FullName',
      parentId: null,
      properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
    };
    const result = createSchemaString(model);
    expect(result).toEqual(
      '{"type":"object","properties":{"FirstName":{"type":"string"},"LastName":{"type":"string"}}}'
    );
  });
});

describe('buildNewObject', () => {
  it('creates a new row object to add to its parent', () => {
    const result = buildNewObject(
      'FieldName',
      {
        displayName: 'string',
        id: '460e33c1-b075-4038-924a-67c213181fcf',
        value: {
          schemaName: 'String',
          schemaString: '{"type": "string", "example": "sample string"}'
        }
      },
      false,
      'field example',
      { parentId: 2 },
      sampleContractString
    );
    expect(result.name).toBe('FieldName');
    expect(result.example).toBe('field example');
    expect(result.type).toBe('string');
  });

  it('creates a new row object for array to add to its parent when isArray is true', () => {
    const result = buildNewObject(
      'FieldName',
      {
        displayName: 'string',
        id: '460e33c1-b075-4038-924a-67c213181fcf',
        value: {
          schemaName: 'String',
          schemaString: '{"type": "string", "example": "sample string"}'
        }
      },
      true,
      'field example',
      { parentId: 2 },
      sampleContractString
    );
    expect(result.name).toBe('FieldName');
    expect(result.example).toBe('field example');
    expect(result.type).toBe('array');
  });

  it('creates a new row object for a guid array to add to its parent when isArray is true', () => {
    const result = buildNewObject(
      'FieldName',
      {
        displayName: 'Guid',
        id: '460e33c1-b075-4038-924a-67c213181fcf',
        value: {
          schemaName: 'Guid',
          schemaString: '{"type": "string", "example": "a sample guid"}'
        }
      },
      true,
      'field example',
      { parentId: 2 },
      sampleContractString
    );
    expect(result.name).toBe('FieldName');
    expect(result.example).toBe('field example');
    expect(result.type).toBe('array');
    expect(result.items.reference).toBe('Guid');
    expect(result.items.$ref).toBe('#/Guid');
  });

  it('maintains the same rowId if editing an existing object', () => {
    const result = buildNewObject(
      'FieldNameEdited',
      {
        displayName: 'string',
        id: '460e33c1-b075-4038-924a-67c213181fcf',
        value: {
          schemaName: 'String',
          schemaString: '{"type": "string", "example": "sample string"}'
        }
      },
      false,
      'field example edited',
      { parentId: 2, name: 'OriginalName', rowId: 5, example: 'original example', type: 'string' },
      sampleContractString
    );
    expect(result.name).toBe('FieldNameEdited');
    expect(result.example).toBe('field example edited');
    expect(result.type).toBe('string');
    expect(result.rowId).toBe(5);
  });

  it('maintains the same rowId if editing an existing array object when isArray is true', () => {
    const result = buildNewObject(
      'FieldNameEdited',
      {
        displayName: 'string',
        id: '460e33c1-b075-4038-924a-67c213181fcf',
        value: {
          schemaName: 'String',
          schemaString: '{"type": "string", "example": "sample string"}'
        }
      },
      true,
      'field example edited',
      { parentId: 2, name: 'OriginalName', rowId: 5, example: 'original example', type: 'string' },
      sampleContractString
    );
    expect(result.name).toBe('FieldNameEdited');
    expect(result.example).toBe('field example edited');
    expect(result.type).toBe('array');
    expect(result.items.reference).toBeUndefined();
    expect(result.items.$ref).toBeUndefined();
    expect(result.rowId).toBe(5);
  });

  it('maintains the same rowId if editing an existing guid array object when isArray is true', () => {
    const result = buildNewObject(
      'FieldNameEdited',
      {
        displayName: 'Guid',
        id: '460e33c1-b075-4038-924a-67c213181fcf',
        value: {
          schemaName: 'Guid',
          schemaString: '{"type": "string", "example": "a sample guid"}'
        }
      },
      true,
      'field example edited',
      { parentId: 2, name: 'OriginalName', rowId: 5, example: 'original example', type: 'string' },
      sampleContractString
    );
    expect(result.name).toBe('FieldNameEdited');
    expect(result.example).toBe('field example edited');
    expect(result.type).toBe('array');
    expect(result.items.reference).toBe('Guid');
    expect(result.items.$ref).toBe('#/Guid');
    expect(result.rowId).toBe(5);
  });
});

describe('findRow', () => {
  it('returns an object with the rowId requested', () => {
    const rows = [
      { rowId: 1, name: 'row1' },
      { rowId: 2, name: 'row2' },
      { rowId: 3, name: 'row3' }
    ];
    const result = findRow(3, rows);
    expect(result.name).toBe('row3');
  });

  it('returns undefined if not found', () => {
    const rows = [{ rowId: 1, name: 'row1' }];
    const result = findRow(3, rows);
    expect(result).toBe(undefined);
  });

  it('finds a row that is double nested', () => {
    const rows = [
      { rowId: 1, name: 'row1' },
      { rowId: 2, name: 'row2' },
      {
        rowId: 3,
        name: 'row3',
        properties: [
          {
            rowId: 4,
            name: 'row4',
            properties: [{ rowId: 7, name: 'row7' }, { rowId: 8, name: 'row8' }]
          },
          { rowId: 5, name: 'row5', properties: [{ rowId: 6, name: 'row6' }] }
        ]
      }
    ];
    const result = findRow(8, rows);
    expect(result.name).toBe('row8');
  });
});

describe('buildNestedOptions', () => {
  it('should attach only objects into the options array with isObject property as true', () => {
    const propertiesArrayString = `
      [
        {
            "type": "string",
            "format": "date-time",
            "example": "2019-01-01T18:14:29Z",
            "name": "Timestamp",
            "rowId": 10
        }, {
            "type": "object",
            "properties": [{
                "type": "integer",
                "example": "123",
                "modalRowId": 1,
                "name": "Field1",
                "rowId": 11
            }],
            "name": "FirstObject",
            "rowId": 12
        }, {
            "type": "object",
            "properties": [{
                "type": "integer",
                "example": "123",
                "modalRowId": 1,
                "name": "Field2",
                "rowId": 13
            }],
            "name": "SecondObject",
            "rowId": 14
        }, {
            "type": "object",
            "properties": [{
                "type": "integer",
                "example": "123",
                "modalRowId": 1,
                "name": "Field3",
                "rowId": 15
            }],
            "name": "ThirdObject",
            "rowId": 16
        }
      ]`;

    const propertiesArray = JSON.parse(propertiesArrayString);
    const optionArray = [];

    buildNestedOptions(propertiesArray, optionArray);

    expect(optionArray.length).toBe(3);
    expect(optionArray[0].displayName).toEqual('FirstObject');
    expect(optionArray[0].isObject).toEqual(true);
    expect(optionArray[1].displayName).toEqual('SecondObject');
    expect(optionArray[1].isObject).toEqual(true);
    expect(optionArray[2].displayName).toEqual('ThirdObject');
    expect(optionArray[2].isObject).toEqual(true);
  });

  it('should attach nested objects ignoring primitives into the options array with isObject property as true', () => {
    const propertiesArrayString = `
      [
        {
            "type": "string",
            "format": "date-time",
            "example": "2019-01-01T18:14:29Z",
            "name": "Timestamp",
            "rowId": 10
        }, {
            "type": "object",
            "properties": [{
                "type": "object",
                "properties": [{
                    "type": "object",
                    "properties": [{
                        "type": "integer",
                        "example": "123",
                        "modalRowId": 1,
                        "name": "Field1",
                        "rowId": 11
                    }, {
                        "type": "integer",
                        "example": "123",
                        "modalRowId": 2,
                        "name": "Field2",
                        "rowId": 13
                    }, {
                        "type": "integer",
                        "example": "123",
                        "modalRowId": 3,
                        "name": "Field3",
                        "rowId": 15
                    }],
                    "name": "ThirdObject",
                    "rowId": 16
                }],
                "name": "SecondObject",
                "rowId": 14
            }],
            "name": "FirstObject",
            "rowId": 12
        }
      ]`;

    const propertiesArray = JSON.parse(propertiesArrayString);
    const optionArray = [];

    buildNestedOptions(propertiesArray, optionArray);

    expect(optionArray.length).toBe(3);
    expect(optionArray[0].displayName).toEqual('FirstObject');
    expect(optionArray[0].isObject).toEqual(true);
    expect(optionArray[1].displayName).toEqual('SecondObject');
    expect(optionArray[1].isObject).toEqual(true);
    expect(optionArray[2].displayName).toEqual('ThirdObject');
    expect(optionArray[2].isObject).toEqual(true);
  });
});

describe('getPropertiesCopy', () => {
  it('should return the properties from an object', () => {
    const model = {
      name: 'FullName',
      properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
    };

    const result = getPropertiesCopy(model);

    expect(result).toEqual([
      { name: 'FirstName', type: 'string' },
      { name: 'LastName', type: 'string' }
    ]);
  });

  it('should return the properties from an object array', () => {
    const model = {
      name: 'FullName',
      items: {
        properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
      }
    };

    const result = getPropertiesCopy(model);

    expect(result).toEqual([
      { name: 'FirstName', type: 'string' },
      { name: 'LastName', type: 'string' }
    ]);
  });

  it('should return an empty array from an object without properties', () => {
    const model = {
      name: 'FullName'
    };

    const result = getPropertiesCopy(model);

    expect(result).toEqual([]);
  });

  it('should return an empty array from an array of objects without properties', () => {
    const model = {
      name: 'FullName',
      items: {}
    };

    const result = getPropertiesCopy(model);

    expect(result).toEqual([]);
  });
});

describe('hasProperties', () => {
  it('should return true from an object with properties', () => {
    const model = {
      name: 'FullName',
      properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
    };

    const result = hasProperties(model);

    expect(result).toEqual(true);
  });

  it('should return true from an array of objects with properties', () => {
    const model = {
      name: 'FullName',
      items: {
        properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
      }
    };

    const result = hasProperties(model);

    expect(result).toEqual(true);
  });

  it('should return false from an object without properties', () => {
    const model = {
      name: 'FullName'
    };

    const result = hasProperties(model);

    expect(result).toEqual(false);
  });

  it('should return false from an array of objects without properties', () => {
    const model = {
      name: 'FullName',
      items: {}
    };

    const result = hasProperties(model);

    expect(result).toEqual(false);
  });
});

describe('isObjectArray', () => {
  it('should return false from an object', () => {
    const model = {
      name: 'FullName',
      properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
    };

    const result = isObjectArray(model);

    expect(result).toEqual(false);
  });

  it('should return true from an array of objects', () => {
    const model = {
      name: 'FullName',
      items: {
        properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
      }
    };

    const result = isObjectArray(model);

    expect(result).toEqual(true);
  });

  it('should return false from an schema object without properties', () => {
    const model = {
      name: 'FullName'
    };

    const result = isObjectArray(model);

    expect(result).toEqual(false);
  });

  it('should return false from an array of objects without properties', () => {
    const model = {
      name: 'FullName',
      items: {}
    };

    const result = isObjectArray(model);

    expect(result).toEqual(false);
  });
});

describe('updateProperties', () => {
  it('should update the properties from an object', () => {
    const model = {
      name: 'FullName',
      properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
    };

    const expectedProperties = [
      { name: 'FirstName', type: 'string' },
      { name: 'MiddleName', type: 'string' },
      { name: 'LastName', type: 'string' }
    ];

    updateProperties(model, expectedProperties);

    expect(model.properties).toEqual(expectedProperties);
  });

  it('should update the properties from an array of objects', () => {
    const model = {
      name: 'FullName',
      items: {
        properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
      }
    };

    const expectedProperties = [
      { name: 'FirstName', type: 'string' },
      { name: 'MiddleName', type: 'string' },
      { name: 'LastName', type: 'string' }
    ];

    updateProperties(model, expectedProperties);

    expect(model.items.properties).toEqual(expectedProperties);
  });

  it('should set the properties in schema.items.properties', () => {
    const model = {
      name: 'FullName',
      properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
    };

    const expectedProperties = [
      { name: 'FirstName', type: 'string' },
      { name: 'MiddleName', type: 'string' },
      { name: 'LastName', type: 'string' }
    ];

    updateProperties(model, expectedProperties, true);

    expect(model.items.properties).toEqual(expectedProperties);
    expect(model.properties).toEqual(undefined);
  });

  it('should set the properties in schema.properties', () => {
    const model = {
      name: 'FullName',
      items: {
        properties: [{ name: 'FirstName', type: 'string' }, { name: 'LastName', type: 'string' }]
      }
    };

    const expectedProperties = [
      { name: 'FirstName', type: 'string' },
      { name: 'MiddleName', type: 'string' },
      { name: 'LastName', type: 'string' }
    ];

    updateProperties(model, expectedProperties, false);

    expect(model.properties).toEqual(expectedProperties);
    expect(model.items).toEqual(undefined);
  });
});
