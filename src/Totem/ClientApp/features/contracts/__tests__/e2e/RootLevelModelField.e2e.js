import { Selector } from 'testcafe';
import { baseUrl } from '../../../../testConfig/setup';
import * as utils from './e2e-utils';

global
  .fixture('Root Level Model Field Tests')
  .page(baseUrl)
  .beforeEach(utils.loginAndNavigateToEditContract)
  .afterEach(utils.logOut);

async function addNewModelAtRoot(t) {
  await t.click(utils.addNewFieldBtn);

  // Add a new model
  await t.typeText(utils.inputFieldName, 'testModel');
  await t.click(utils.inputType).click(Selector('li').withText('Define a new model'));

  // Add a new field to the model
  await t.click(utils.addNewFieldNestedBtn);
  await t.typeText(utils.inputFieldName, 'testProperty');
  await t.click(utils.inputType).click(Selector('li').withText('Integer'));
  await t.click(utils.saveFieldBtn);

  // Save the model
  const saveModelBtn = Selector('#saveModelBtn');
  await t.click(saveModelBtn);
}

test('Add a new model field at the root', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  await addNewModelAtRoot(t);

  const newlyAddedModel = Selector('tr.treegrid-body-row').withText('testModel');
  const newlyAddedNestedRow = Selector('tr.treegrid-body-row').withText('testProperty');
  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 2)
    .expect(newlyAddedModel.exists)
    .eql(true)
    .expect(newlyAddedNestedRow.exists)
    .eql(true);
});

test('Cancel creating a model', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;
  await t.click(utils.addNewFieldBtn);

  // Add a new model
  await t.typeText(utils.inputFieldName, 'testModel');
  await t.click(utils.inputType).click(Selector('li').withText('Define a new model'));

  const cancelBtn = Selector('#modelModalWindow').find('#cancelBtn');
  await t.click(cancelBtn);

  const newlyAddedModel = Selector('tr.treegrid-body-row').withText('testModel');
  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(newlyAddedModel.exists)
    .eql(false);
});

test('Edit root level model name', async t => {
  await addNewModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const objectRowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = objectRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  await t.typeText(utils.modelName, 'editTestModel', { replace: true });

  const saveModelBtn = Selector('#saveModelBtn');
  await t.click(saveModelBtn);

  const oldModelRow = Selector('tr.treegrid-body-row').withText('testModel');
  const newModelRow = Selector('tr.treegrid-body-row').withText('editTestModel');
  const nestedRow = Selector('tr.treegrid-body-row').withText('testProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(oldModelRow.exists)
    .eql(false)
    .expect(newModelRow.exists)
    .eql(true)
    .expect(nestedRow.exists)
    .eql(true);
});

test('Cancel edits on model name', async t => {
  await addNewModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const objectRowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = objectRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  await t.typeText(utils.modelName, 'editTestModel', { replace: true });

  const cancelBtn = Selector('#modelModalWindow').find('#cancelBtn');
  await t.click(cancelBtn);

  const oldModelRow = Selector('tr.treegrid-body-row').withText('testModel');
  const newModelRow = Selector('tr.treegrid-body-row').withText('editTestModel');
  const nestedRow = Selector('tr.treegrid-body-row').withText('testProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(oldModelRow.exists)
    .eql(true)
    .expect(newModelRow.exists)
    .eql(false)
    .expect(nestedRow.exists)
    .eql(true);
});

test('Delete a model at root level', async t => {
  await addNewModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const objectRowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = objectRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const deleteModelBtn = Selector('#modelModalWindow').find('#deleteModelBtn');
  await t.click(deleteModelBtn);

  const deletedModelRow = Selector('tr.treegrid-body-row').withText('testModel');
  const deletedNestedRow = Selector('tr.treegrid-body-row').withText('testProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount - 2)
    .expect(deletedModelRow.exists)
    .eql(false)
    .expect(deletedNestedRow.exists)
    .eql(false);
});

test('Prevent deleting the only child field on a parent model root field', async t => {
  await addNewModelAtRoot(t);

  // Finding a root field to delete
  const fieldToBeDeleted = Selector('tr.treegrid-body-row').withText('testProperty');
  const editFieldBtn = fieldToBeDeleted.find('.edit-action');

  await t.click(editFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  // Cannot delete the only child of a parent root field
  await t.expect(utils.deleteFieldBtn.hasAttribute('disabled')).ok();
});

test('Add a new model using previous model', async t => {
  await addNewModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  await t.click(utils.addNewFieldBtn);

  // Add a new field based on previous model
  await t.typeText(utils.inputFieldName, 'testNewModel');
  await t.click(utils.inputType).click(Selector('li').withText('testModel'));
  await t.expect(utils.inputFieldExample.hasAttribute('disabled')).ok();

  await t.click(utils.saveFieldBtn);

  const newlyAddedModelField = Selector('tr.treegrid-body-row').withText('testNewModel');
  const nestedRows = await Selector('tr.treegrid-body-row').withText('testProperty');
  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 2)
    .expect(newlyAddedModelField.exists)
    .eql(true)
    .expect(nestedRows.exists)
    .eql(true)
    .expect(nestedRows.count)
    .eql(2);
});

test('Edit a non-root level field from the root table', async t => {
  await addNewModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const nestedRowToEdit = Selector('tr.treegrid-body-row').withText('testProperty');
  const editFieldBtn = nestedRowToEdit.find('.edit-action');

  await t.click(editFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  await t.typeText(utils.inputFieldName, 'nestedPropertyEdited', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));
  await t.typeText(utils.inputFieldExample, 'nestedPropertyExample', { replace: true });

  await t.click(utils.saveFieldBtn);

  const oldModelRow = Selector('tr.treegrid-body-row').withText('testModel');
  const oldNestedRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const newNestedRow = Selector('tr.treegrid-body-row').withText('nestedPropertyEdited');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(oldModelRow.exists)
    .eql(true)
    .expect(newNestedRow.exists)
    .eql(true)
    .expect(newNestedRow.textContent)
    .contains('date-time')
    .expect(newNestedRow.textContent)
    .contains('nestedPropertyExample')
    .expect(oldNestedRow.exists)
    .eql(false);
});
