import { reorderOptions, getDisplayType, deepCopy } from '../dataHelpers';

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
