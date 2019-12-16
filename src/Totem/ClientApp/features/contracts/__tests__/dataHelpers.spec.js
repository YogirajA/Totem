import {
  reorderOptions,
  getDisplayType,
  deepCopy,
  findRowInTreeAndUpdate,
  last,
  findRowInTreeAndDelete,
  findParent,
  isValidJSON,
  isDate,
  isGUID,
  isFloat,
  isDouble,
  isInt32,
  isInt64,
  isNumber,
  isBool
} from '../dataHelpers';

describe('Reorder Options', () => {
  it('should sort the an array alphabetically except for the "Define new" option at the end', () => {
    const options = [
      { displayName: 'ZZZ', id: '1a2936f3-b3e1-4395-9b74-3b77f647537e' },
      { displayName: 'AAA', id: 54 },
      { displayName: 'BBB', id: 15 }
    ];

    const result = reorderOptions(options);

    expect(Array.isArray(result)).toBe(true);
    expect(result.length).toBe(4);
    expect(result[0].displayName).toBe('AAA');
    expect(result[2].displayName).toBe('ZZZ');
    expect(result[3].displayName).toBe('Define a new model');
    expect(result[3].id).toBe(0);
  });

  it('should not have two "Define new" options even if the starting array has one', () => {
    const options = [
      { displayName: 'AAA', id: '1a2936f3-b3e1-4395-9b74-3b77f647537e' },
      { displayName: 'Define a new model', id: 0, value: null }
    ];

    const result = reorderOptions(options);

    expect(Array.isArray(result)).toBe(true);
    expect(result.length).toBe(2);
    expect(result[0].displayName).toBe('AAA');
    expect(result[1].displayName).toBe('Define a new model');
    expect(result[1].id).toBe(0);
  });
});

describe('Get Display Type', () => {
  it('returns the lower case reference name if it has a reference type', () => {
    const property = {
      type: 'string',
      example: 'test',
      reference: 'RefName'
    };
    const result = getDisplayType(property);
    expect(result).toBe('refname');
  });

  it('returns the type and format if a format is defined', () => {
    const property = {
      type: 'string',
      format: 'date-time',
      example: 'test'
    };
    const result = getDisplayType(property);
    expect(result).toBe('string (date-time)');
  });

  it('returns the type if no format or reference defined', () => {
    const property = {
      type: 'string',
      example: 'test'
    };
    const result = getDisplayType(property);
    expect(result).toBe('string');
  });

  it('returns object if no type defined', () => {
    const property = {};
    const result = getDisplayType(property);
    expect(result).toBe('object');
  });
});

describe('deepCopy', () => {
  it("returns a copy of the original object/array that won't mutate the original when assigned", () => {
    const originalObject = { rowId: 1, name: 'row1', properties: [{ rowId: 2, name: 'row2' }] };
    const deepCopyObject = deepCopy(originalObject);
    deepCopyObject.properties[0].name = 'modifiedRow';
    expect(originalObject.properties[0].name).toBe('row2');
  });
});

describe('findRowInTreeAndUpdate', () => {
  it('returns undefined if the rowId is not found anywhere in the tree', () => {
    const originalTrees = [{ rowId: 1, name: 'row1', properties: [{ rowId: 2, name: 'row2' }] }];
    const newObject = { rowId: 3, name: 'row3' };

    const updatedObject = findRowInTreeAndUpdate(originalTrees, newObject);
    expect(updatedObject).toBe(undefined);
  });

  it('returns a modified tree if the rowId is found anywhere in the tree', () => {
    const originalTrees = [
      {
        rowId: 1,
        name: 'row1',
        properties: [
          { rowId: 2, name: 'row2', properties: [{ rowId: 4, name: 'row4' }] },
          { rowId: 5, name: 'row5', properties: [{ rowId: 3, name: 'row3' }] }
        ]
      },
      {
        rowId: 6,
        name: 'row6',
        properties: [
          { rowId: 7, name: 'row7', properties: [{ rowId: 8, name: 'row8' }] },
          { rowId: 9, name: 'row9', properties: [{ rowId: 10, name: 'row10' }] }
        ]
      }
    ];
    const newObject = { rowId: 10, name: 'newRow10' };

    const updatedObject = findRowInTreeAndUpdate(originalTrees, newObject);
    expect(updatedObject[1].properties[1].properties[0].name).toBe('newRow10');
  });
});

describe('findRowInTreeAndDelete', () => {
  it('returns undefined if the rowId is not found anywhere in the tree', () => {
    const originalTree = [{ rowId: 1, name: 'row1', properties: [{ rowId: 2, name: 'row2' }] }];
    const objectToDelete = { rowId: 3, name: 'row3' };

    const updatedTree = findRowInTreeAndDelete(originalTree, objectToDelete);
    expect(updatedTree).toBe(undefined);
  });

  it('returns a modified tree if the rowId is found anywhere in the tree', () => {
    const originalTree = [
      {
        rowId: 1,
        name: 'row1',
        properties: [
          { rowId: 2, name: 'row2', properties: [{ rowId: 4, name: 'row4' }] },
          { rowId: 5, name: 'row5', properties: [{ rowId: 3, name: 'row3' }] }
        ]
      },
      {
        rowId: 6,
        name: 'row6',
        properties: [
          { rowId: 7, name: 'row7', properties: [{ rowId: 8, name: 'row8' }] },
          {
            rowId: 9,
            name: 'row9',
            properties: [{ rowId: 10, name: 'row10' }, { rowId: 11, name: 'row11' }]
          }
        ]
      }
    ];
    const objectToDelete = { rowId: 10, name: 'row10' };

    const updatedTree = findRowInTreeAndDelete(originalTree, objectToDelete);
    expect(updatedTree[1].properties[1].properties.length).toBe(1);
    expect(updatedTree[1].properties[1].properties[0].name).toBe('row11');
  });
});

describe('findParent', () => {
  const tree = [
    {
      rowId: 1,
      name: 'row1',
      properties: [
        { rowId: 2, name: 'row2', properties: [{ rowId: 4, name: 'row4' }] },
        { rowId: 5, name: 'row5', properties: [{ rowId: 3, name: 'row3' }] }
      ]
    },
    {
      rowId: 6,
      name: 'row6',
      properties: [
        { rowId: 7, name: 'row7', properties: [{ rowId: 8, name: 'row8' }] },
        {
          rowId: 9,
          name: 'row9',
          properties: [{ rowId: 10, name: 'row10' }, { rowId: 11, name: 'row11' }]
        }
      ]
    }
  ];

  it('returns the parent of the the given child row in a tree', () => {
    const childRow = { rowId: 11, name: 'row11' };
    const parentRow = findParent(tree, childRow);
    expect(parentRow.name).toBe('row9');
  });

  it('returns null if the child row is a root grid property (top level of nesting)', () => {
    const childRow = { rowId: 1, name: 'row1' };
    const parentRow = findParent(tree, childRow);
    expect(parentRow).toBe(null);
  });
});

describe('last', () => {
  it('returns the last item in an array', () => {
    const array = [{ name: '1' }, { name: '2' }, { name: '3' }];
    const item = last(array);
    expect(item.name).toBe('3');
  });

  it('returns undefined if the array is empty', () => {
    const array = [];
    const item = last(array);
    expect(item).toBe(undefined);
  });
});

describe('isValidJSON', () => {
  it('returns true when string is valid JSON', () => {
    const testString = `{
      "item1": "test",
      "item2": {
        "item3": "test543",
        "item4": {
          "item5": "testu436"
        }
      }
    }`;
    const result = isValidJSON(testString);
    expect(result).toBe(true);
  });

  it('returns false when string is not valid JSON', () => {
    const testString = 'invalid string';
    const result = isValidJSON(testString);
    expect(result).toBe(false);
  });
});

describe('isDate', () => {
  it('returns true when string is valid Date', () => {
    const testString = `2019-01-01T18:14:29Z`;
    const result = isDate(testString);
    expect(result).toBe(true);
  });

  it('returns false when string is not valid Date', () => {
    const testString = 'not a date';
    const result = isDate(testString);
    expect(result).toBe(false);
  });

  it('returns false when string number is not valid Date', () => {
    const testString = '5.5';
    const result = isDate(testString);
    expect(result).toBe(false);
  });
});

describe('isGUID', () => {
  it('returns true when string is valid GUID', () => {
    const testString = `01234567-abcd-0123-abcd-0123456789ab`;
    const result = isGUID(testString);
    expect(result).toBe(true);
  });

  it('returns false when string is not valid GUID', () => {
    const testString = 'not a guid';
    const result = isGUID(testString);
    expect(result).toBe(false);
  });
});

describe('isNumber', () => {
  it('returns true when parameter is a number', () => {
    const testValue = 3;
    const result = isNumber(testValue);
    expect(result).toBe(true);
  });

  it('returns false when value is not numeric', () => {
    const testValue = 'not a number';
    const result = isNumber(testValue);
    expect(result).toBe(false);
  });

  it('returns false when number is a string', () => {
    const testValue = '3';
    const result = isNumber(testValue);
    expect(result).toBe(false);
  });
});

describe('isInt32', () => {
  it('returns true when parameter is an int32 integer', () => {
    const testValue = 4;
    const result = isInt32(testValue);
    expect(result).toBe(true);
  });

  it('returns false when value is not an int32 integer', () => {
    const testValue = 2147483657;
    const result = isInt32(testValue);
    expect(result).toBe(false);
  });

  it('returns false when an int32 integer is a string', () => {
    const testValue = '4';
    const result = isInt32(testValue);
    expect(result).toBe(false);
  });
});

describe('isInt64', () => {
  it('returns true when parameter is an int64 integer', () => {
    const testValue = 2147483648;
    const result = isInt64(testValue);
    expect(result).toBe(true);
  });

  it('returns false when value is not an int64 integer', () => {
    const testValue = 'not an int64';
    const result = isInt64(testValue);
    expect(result).toBe(false);
  });

  it('returns false when an int64 integer is a string', () => {
    const testValue = '2147483648';
    const result = isInt64(testValue);
    expect(result).toBe(false);
  });
});

describe('isFloat', () => {
  it('returns true when value is a float', () => {
    const testValue = 10.25;
    const result = isFloat(testValue);
    expect(result).toBe(true);
  });

  it('returns false when value is not a float', () => {
    const testValue = 'not a float';
    const result = isFloat(testValue);
    expect(result).toBe(false);
  });

  it('returns false when a float is a string', () => {
    const testValue = `10.25`;
    const result = isFloat(testValue);
    expect(result).toBe(false);
  });

  it('returns false when value is a double', () => {
    const testValue = 10.25e55;
    const result = isFloat(testValue);
    expect(result).toBe(false);
  });
});

describe('isDouble', () => {
  it('returns true when value is a double', () => {
    const testValue = 10.25e55;
    const result = isDouble(testValue);
    expect(result).toBe(true);
  });

  it('returns false when value is not a double', () => {
    const testValue = 'not a double';
    const result = isDouble(testValue);
    expect(result).toBe(false);
  });

  it('returns false when a double is a string', () => {
    const testValue = `10.25e34`;
    const result = isDouble(testValue);
    expect(result).toBe(false);
  });
});

describe('isBool', () => {
  it('returns true when string is a bool', () => {
    const testValue = true;
    const result = isBool(testValue);
    expect(result).toBe(true);
  });

  it('returns false when string is not a bool', () => {
    const testValue = 'not a bool';
    const result = isBool(testValue);
    expect(result).toBe(false);
  });

  it('returns false when a bool is a string', () => {
    const testValue = `true`;
    const result = isBool(testValue);
    expect(result).toBe(false);
  });
});
