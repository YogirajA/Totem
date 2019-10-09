/* eslint-disable no-restricted-syntax */
import VueSelector from 'testcafe-vue-selectors';
import { Selector } from 'testcafe';
import { baseUrl } from '../../../../testConfig/setup';
import * as utils from './e2e-utils';

async function addNewFieldAtRoot(t) {
  await t.click(utils.addNewFieldBtn);

  await t.typeText(utils.inputFieldName, 'testProperty');
  await t.click(utils.inputType).click(Selector('li').withText('Integer'));

  await t.click(utils.saveFieldBtn);
}

global
  .fixture('Root Level Single Field Tests')
  .page(baseUrl)
  .beforeEach(utils.loginAndNavigateToEditContract)
  .afterEach(utils.logOut);

test('Root contract grid visible', async t => {
  const rootApp = await VueSelector();
  await t.expect(rootApp.visible).eql(true);
});

test('Add a new field at the root', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  await addNewFieldAtRoot(t);

  const newlyAddedRow = Selector('tr.treegrid-body-row').withText('testProperty');
  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 1)
    .expect(newlyAddedRow.exists)
    .eql(true);
});

test('Cancel adding a field', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;
  await t.click(utils.addNewFieldBtn);

  await t.typeText(utils.inputFieldName, 'testProperty');
  await t.click(utils.inputType).click(Selector('li').withText('Integer'));

  await t.click(utils.cancelFieldBtn);
  await t.expect(Selector('tr.treegrid-body-row').count).eql(initialRowCount);

  await t.click(utils.addNewFieldBtn);
  await t.expect(utils.inputFieldName.value).eql('');
  await t.expect(utils.inputFieldExample.value).eql('');
  await t.expect(utils.inputType.getVue(({ props }) => props.value)).eql(null);
});

test('Edit a root field', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  // Adding a field to edit
  await addNewFieldAtRoot(t);

  const newlyAddedRow = Selector('tr.treegrid-body-row').nth(-1);
  const editFieldBtn = newlyAddedRow.find('.edit-action');

  await t.click(editFieldBtn);
  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  await t.expect(Selector('tr.treegrid-body-row').count).eql(initialRowCount + 1);

  // Editing the field
  await t.typeText(utils.inputFieldName, 'editProperty', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));

  // Saving the edit
  await t.click(utils.saveFieldBtn);

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 1)
    .expect(newlyAddedRow.textContent)
    .contains('editProperty')
    .expect(newlyAddedRow.textContent)
    .contains('date-time');
});

test('Cancel editing a root field', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  // Adding a field to edit
  await addNewFieldAtRoot(t);

  const newlyAddedRow = Selector('tr.treegrid-body-row').nth(-1);
  const editFieldBtn = newlyAddedRow.find('.edit-action');

  await t.click(editFieldBtn);
  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  await t.expect(Selector('tr.treegrid-body-row').count).eql(initialRowCount + 1);

  // Editing the field
  await t.typeText(utils.inputFieldName, 'editProperty', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));

  // Canceling the edit
  await t.click(utils.cancelFieldBtn);

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 1)
    .expect(newlyAddedRow.textContent)
    .contains('testProperty')
    .expect(newlyAddedRow.textContent)
    .contains('integer');
});

test('Saving a guid field shows the correct data model in the dropdown when editing', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  // Adding a field to edit
  await t.click(utils.addNewFieldBtn);
  await t.typeText(utils.inputFieldName, 'guidProperty');
  await t.click(utils.inputType).click(Selector('li').withText('Guid'));

  await t.click(utils.saveFieldBtn);
  const newlyAddedRow = Selector('tr.treegrid-body-row').nth(-1);
  const editFieldBtn = newlyAddedRow.find('.edit-action');

  await t.click(editFieldBtn);
  await t.expect(utils.inputFieldName.value).eql('guidProperty');
  await t.expect(utils.inputFieldExample.value).eql('01234567-abcd-0123-abcd-0123456789ab');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Guid');

  await t.expect(Selector('tr.treegrid-body-row').count).eql(initialRowCount + 1);
});

test('Saving a datetime field shows the correct data model in the dropdown when editing', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  // Adding a field to edit
  await t.click(utils.addNewFieldBtn);
  await t.typeText(utils.inputFieldName, 'dateTimeProperty');
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));

  await t.click(utils.saveFieldBtn);
  const newlyAddedRow = Selector('tr.treegrid-body-row').nth(-1);
  const editFieldBtn = newlyAddedRow.find('.edit-action');

  await t.click(editFieldBtn);
  await t.expect(utils.inputFieldName.value).eql('dateTimeProperty');
  await t.expect(utils.inputFieldExample.value).eql('2019-01-01T18:14:29Z');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('DateTime');

  await t.expect(Selector('tr.treegrid-body-row').count).eql(initialRowCount + 1);
});

test('Deleting a previously saved root field', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  // Finding a root field to delete
  const fieldToBeDeleted = Selector('tr.treegrid-body-row').withText('integer');
  const editFieldBtn = fieldToBeDeleted.find('.edit-action');

  await t.click(editFieldBtn);
  await t.expect(utils.inputFieldName.value).notEql('');
  await t.expect(utils.inputFieldExample.value).notEql('');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  // Deleting the field
  await t.click(utils.deleteFieldBtn);

  const fieldName = utils.inputFieldName.innerText.toString();
  const deletedRow = Selector('tr.treegrid-body-row').withText(fieldName);
  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount - 1)
    .expect(deletedRow.exists)
    .eql(false);
});

test('Delete all root fields', async t => {
  const rowCount = await Selector('tr.treegrid-body-row').count;

  for (let i = 0; i < rowCount; i++) {
    await t.click(
      Selector('tr.treegrid-body-row')
        .nth(0)
        .find('.edit-action')
    );
    await t.click(utils.deleteFieldBtn);
  }

  await t.expect(Selector('tr.treegrid-body-row').count).eql(0);
  await t.expect(Selector('#rootGrid .treegrid-empty-row').count).eql(1);
});

test('Adding and deleting a root field', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  // Adding a field to delete
  await addNewFieldAtRoot(t);

  const newlyAddedRow = Selector('tr.treegrid-body-row').nth(-1);
  const editFieldBtn = newlyAddedRow.find('.edit-action');

  await t.click(editFieldBtn);
  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  await t.expect(Selector('tr.treegrid-body-row').count).eql(initialRowCount + 1);

  // Deleting the field
  await t.click(utils.deleteFieldBtn);

  const deletedRow = Selector('tr.treegrid-body-row').withText('testProperty');
  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(deletedRow.exists)
    .eql(false);
});

test('Edit Contract Manually', async t => {
  const dropDownMenu = Selector('.dropdown-toggle');
  const editManuallyBtn = Selector('.edit-manually');

  const contractTextArea = Selector('#contract-raw');

  // Editing the contract string
  const newContractString = JSON.stringify({
    Contract: {
      type: 'object',
      properties: {
        Id: {
          $ref: '#/Guid',
          example: '01234567-abcd-0123-abcd-0123456789ab'
        },
        Timestamp: {
          type: 'string',
          format: 'date-time',
          example: '2019-01-01T18:14:29Z'
        },
        Name: {
          type: 'string'
        }
      }
    },
    Guid: {
      type: 'string',
      pattern: '^(([0-9a-f]){8}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){4}-([0-9a-f]){12})$',
      minLength: 36,
      maxLength: 36,
      example: '01234567-abcd-0123-abcd-0123456789ab'
    }
  });
  await t.click(dropDownMenu).click(editManuallyBtn);
  await t
    .selectText(contractTextArea)
    .pressKey('backspace')
    .typeText(contractTextArea, newContractString);

  const updateContractBtn = Selector('#updateContract');

  // Saving the contract string
  await t.click(updateContractBtn);

  const firstRow = Selector('tr.treegrid-body-row').nth(0);
  const secondRow = Selector('tr.treegrid-body-row').nth(1);
  const thirdRow = Selector('tr.treegrid-body-row').nth(2);

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(3)
    .expect(firstRow.textContent)
    .contains('Id')
    .expect(secondRow.textContent)
    .contains('Timestamp')
    .expect(thirdRow.textContent)
    .contains('Name');
});
