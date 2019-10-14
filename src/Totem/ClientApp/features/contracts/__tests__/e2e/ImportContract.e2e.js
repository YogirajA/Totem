import { Selector } from 'testcafe';
import { baseUrl } from '../../../../testConfig/setup';
import * as utils from './e2e-utils';

global
  .fixture('Import Contract Test')
  .page(baseUrl)
  .beforeEach(utils.loginAndNavigateToEditContract)
  .afterEach(utils.logOut);

test('Import a non-nested contract', async t => {
  await t.click(utils.importContractBtn);

  await t.expect(utils.importTextArea.value).eql('');

  const messageString = `{
    "item1": "test1",
    "item2": "test2",
    "item3": "test3"
  }`;
  await t.typeText(utils.importTextArea, messageString, { replace: true });
  await t.click(utils.importBtn);

  const item1Row = Selector('tr.treegrid-body-row').withText('item1');
  const item2Row = Selector('tr.treegrid-body-row').withText('item2');
  const item3Row = Selector('tr.treegrid-body-row').withText('item3');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(3)
    .expect(item1Row.exists)
    .eql(true)
    .expect(item2Row.exists)
    .eql(true)
    .expect(item3Row.exists)
    .eql(true);
});

test('Import a contract with string formats', async t => {
  await t.click(utils.importContractBtn);

  await t.expect(utils.importTextArea.value).eql('');

  const messageString = `{
    "item1": "2019-01-01T18:14:29Z",
    "item2": "01234567-abcd-0123-abcd-0123456789ab",
    "item3": "test3"
  }`;
  await t.typeText(utils.importTextArea, messageString, { replace: true });
  await t.click(utils.importBtn);

  const item1Row = Selector('tr.treegrid-body-row').withText('item1');
  const item2Row = Selector('tr.treegrid-body-row').withText('item2');
  const item3Row = Selector('tr.treegrid-body-row').withText('item3');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(3)
    .expect(item1Row.exists)
    .eql(true)
    .expect(item1Row.textContent)
    .contains('string (date-time)')
    .expect(item2Row.exists)
    .eql(true)
    .expect(item2Row.textContent)
    .contains('guid')
    .expect(item3Row.exists)
    .eql(true)
    .expect(item3Row.textContent)
    .contains('string');
});

test('Import a contract with numeric formats', async t => {
  await t.click(utils.importContractBtn);

  await t.expect(utils.importTextArea.value).eql('');

  const messageString = `{
    "item1": 3,
    "item2": 2147483650,
    "item3": 5.5
  }`;
  await t.typeText(utils.importTextArea, messageString, { replace: true });
  await t.click(utils.importBtn);

  const item1Row = Selector('tr.treegrid-body-row').withText('item1');
  const item2Row = Selector('tr.treegrid-body-row').withText('item2');
  const item3Row = Selector('tr.treegrid-body-row').withText('item3');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(3)
    .expect(item1Row.exists)
    .eql(true)
    .expect(item1Row.textContent)
    .contains('integer (int32)')
    .expect(item2Row.exists)
    .eql(true)
    .expect(item2Row.textContent)
    .contains('integer (int64)')
    .expect(item3Row.exists)
    .eql(true)
    .expect(item3Row.textContent)
    .contains('number (float)');
});

test('Import a nested contract', async t => {
  await t.click(utils.importContractBtn);

  await t.expect(utils.importTextArea.value).eql('');

  const messageString = `{
    "item1": "test",
    "item2": {
      "item3": "test543",
      "item4": {
        "item5": "testu436"
      }
    }
  }`;
  await t.typeText(utils.importTextArea, messageString, { replace: true });
  await t.click(utils.importBtn);

  const item1Row = Selector('tr.treegrid-body-row').withText('item1');
  const item2Row = Selector('tr.treegrid-body-row').withText('item2');
  const item3Row = Selector('tr.treegrid-body-row').withText('item3');
  const item4Row = Selector('tr.treegrid-body-row').withText('item4');
  const item5Row = Selector('tr.treegrid-body-row').withText('item5');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(5)
    .expect(item1Row.exists)
    .eql(true)
    .expect(item2Row.exists)
    .eql(true)
    .expect(item2Row.textContent)
    .contains('object')
    .expect(item3Row.exists)
    .eql(true)
    .expect(item4Row.exists)
    .eql(true)
    .expect(item4Row.textContent)
    .contains('object')
    .expect(item5Row.exists)
    .eql(true);
});

test('Import a contract with array', async t => {
  await t.click(utils.importContractBtn);

  await t.expect(utils.importTextArea.value).eql('');

  const messageString = `{
    "item1": "test",
    "item2": ["string1", "string2"]
  }`;
  await t.typeText(utils.importTextArea, messageString, { replace: true });
  await t.click(utils.importBtn);

  const item1Row = Selector('tr.treegrid-body-row').withText('item1');
  const item2Row = Selector('tr.treegrid-body-row').withText('item2');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(2)
    .expect(item1Row.exists)
    .eql(true)
    .expect(item2Row.exists)
    .eql(true)
    .expect(item2Row.textContent)
    .contains('array (string)');
});
