import { Selector } from 'testcafe';
import { baseUrl } from '../../../../testConfig/setup';
import * as utils from './e2e-utils';

global
  .fixture('Nested Model Field Tests')
  .page(baseUrl)
  .beforeEach(utils.loginAndNavigateToEditContract)
  .afterEach(utils.logOut);

async function addNewNestedModelAtRoot(t, keepModalOpen = true) {
  await t.click(utils.addNewFieldBtn);

  // Add a new container model
  await t.typeText(utils.inputFieldName, 'testModel');
  await t.click(utils.inputType).click(Selector('li').withText('Define a new model'));

  // Add a new nested model to the model
  await t.click(utils.addNewFieldNestedBtn);
  await t.typeText(utils.inputFieldName, 'nestedModel');
  await t.click(utils.inputType).click(Selector('li').withText('Define a new model'));

  // Add a field to the nested model
  await t.click(utils.addNewFieldNestedBtn);
  await t.typeText(utils.inputFieldName, 'testProperty');
  await t.click(utils.inputType).click(Selector('li').withText('Integer'));

  await t.click(utils.saveFieldBtn);

  // Save the nested model
  await t.click(utils.saveModelBtn);

  if (keepModalOpen === true) {
    // Save the container model
    await t.click(utils.saveModelBtn);
  }
}

async function addNewFieldsToNestedModel(t) {
  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  await t.click(utils.addNewFieldNestedBtn);

  await t.typeText(utils.inputFieldName, 'newNestedProperty', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));

  await t.click(utils.saveFieldBtn);
}

async function addNewFieldsToParentModel(t) {
  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  await t.click(utils.addNewFieldNestedBtn);

  await t.typeText(utils.inputFieldName, 'newTestProperty', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));

  await t.click(utils.saveFieldBtn);
}

test('Add a new nested model at the root', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  await addNewNestedModelAtRoot(t);

  const newlyAddedModel = Selector('tr.treegrid-body-row').withText('testModel');
  const newlyAddedNestedModel = Selector('tr.treegrid-body-row').withText('nestedModel');
  const newlyAddedNestedRow = Selector('tr.treegrid-body-row').withText('testProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 3)
    .expect(newlyAddedModel.exists)
    .eql(true)
    .expect(newlyAddedNestedModel.exists)
    .eql(true)
    .expect(newlyAddedNestedRow.exists)
    .eql(true);
});

test('Edit a 2x nested model name from root grid', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const objectRowToEdit = Selector('tr.treegrid-body-row').withText('nestedModel');
  const editFieldBtn = objectRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  await t.typeText(utils.modelName, 'editNestedModel', { replace: true });

  await t.click(utils.saveModelBtn);

  const oldContainerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const oldNestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const newNestedModelRow = Selector('tr.treegrid-body-row').withText('editNestedModel');
  const nestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(oldNestedModelRow.exists)
    .eql(false)
    .expect(oldContainerModel.exists)
    .eql(true)
    .expect(newNestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRow.exists)
    .eql(true);
});

test('Edit a 2x nested model name from modal window', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const objectRowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = objectRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  await t.typeText(utils.modelName, 'editNestedModel', { replace: true });

  // Save the nested model
  await t.click(utils.saveModelBtn);

  // Save the container model
  await t.click(utils.saveModelBtn);

  const oldContainerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const oldNestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const newNestedModelRow = Selector('tr.treegrid-body-row').withText('editNestedModel');
  const nestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(oldNestedModelRow.exists)
    .eql(false)
    .expect(oldContainerModel.exists)
    .eql(true)
    .expect(newNestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRow.exists)
    .eql(true);
});

test('Create a nested model then edit a 2x nested model name without closing the modal window', async t => {
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  await addNewNestedModelAtRoot(t, false);

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  await t.typeText(utils.modelName, 'editNestedModel', { replace: true });

  // Save the nested model
  await t.click(utils.saveModelBtn);

  // Save the container model
  await t.click(utils.saveModelBtn);

  const oldContainerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const oldNestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const newNestedModelRow = Selector('tr.treegrid-body-row').withText('editNestedModel');
  const nestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 3)
    .expect(oldNestedModelRow.exists)
    .eql(false)
    .expect(oldContainerModel.exists)
    .eql(true)
    .expect(newNestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRow.exists)
    .eql(true);
});

test('Edit a 2x nested field from root grid', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const nestedFieldRowToEdit = Selector('tr.treegrid-body-row').withText('testProperty');
  const editFieldBtn = nestedFieldRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  await t.typeText(utils.inputFieldName, 'nestedPropertyEdited', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));
  await t.typeText(utils.inputFieldExample, 'nestedPropertyExample', { replace: true });

  await t.click(utils.saveFieldBtn);

  const oldContainerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const oldNestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const oldNestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const newNestedPropertyRow = Selector('tr.treegrid-body-row').withText('nestedPropertyEdited');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(oldContainerModel.exists)
    .eql(true)
    .expect(oldNestedModelRow.exists)
    .eql(true)
    .expect(oldNestedPropertyRow.exists)
    .eql(false)
    .expect(newNestedPropertyRow.exists)
    .eql(true);
});

test('Cancel adding a new field to the model (from the parent model)', async t => {
  await addNewNestedModelAtRoot(t);

  const nestedFieldRowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = nestedFieldRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  await t.click(utils.addNewFieldNestedBtn);

  await t.typeText(utils.inputFieldName, 'newProperty', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('Integer'));

  await t.click(utils.cancelFieldBtn);

  await t.click(utils.addNewFieldNestedBtn);
  await t.expect(utils.inputFieldName.value).eql('');
  await t.expect(utils.inputFieldExample.value).eql('');
  await t.expect(utils.inputType.getVue(({ props }) => props.value)).eql(null);
});

test('Edit a pre-existing child row in the modal window (from the parent model)', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const nestedFieldRowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = nestedFieldRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const childRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('testProperty');
  const childRowEditFieldBtn = childRowToEdit.find('.edit-action');
  await t.click(childRowEditFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  await t.typeText(utils.inputFieldName, 'nestedPropertyEdited', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));
  await t.typeText(utils.inputFieldExample, 'nestedPropertyExample', { replace: true });

  await t.click(utils.saveFieldBtn);

  await t.click(utils.saveModelBtn);

  const oldContainerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const oldNestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const oldNestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const editedNestedPropertyRow = Selector('tr.treegrid-body-row').withText('nestedPropertyEdited');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(oldContainerModel.exists)
    .eql(true)
    .expect(oldNestedModelRow.exists)
    .eql(true)
    .expect(oldNestedPropertyRow.exists)
    .eql(false)
    .expect(editedNestedPropertyRow.exists)
    .eql(true)
    .expect(editedNestedPropertyRow.textContent)
    .contains('date-time')
    .expect(editedNestedPropertyRow.textContent)
    .contains('nestedPropertyExample');
});

test('Edit a newly added child row before closing the modal window (from the parent model)', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const nestedFieldRowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = nestedFieldRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  await t.click(utils.addNewFieldNestedBtn);

  await t.typeText(utils.inputFieldName, 'newProperty', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('Integer'));

  await t.click(utils.saveFieldBtn);

  const childRowToEdit = Selector('tr.treegrid-body-row').withText('newProperty');
  const childRowEditFieldBtn = childRowToEdit.find('.edit-action');
  await t.click(childRowEditFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('newProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  await t.typeText(utils.inputFieldName, 'newlyAddedNestedPropertyEdited', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));
  await t.typeText(utils.inputFieldExample, 'newlyAddedNestedPropertyExample', { replace: true });

  await t.click(utils.saveFieldBtn);

  await t.click(utils.saveModelBtn);

  const oldContainerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const oldNestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const oldNestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const newNestedPropertyRow = Selector('tr.treegrid-body-row').withText(
    'newlyAddedNestedPropertyEdited'
  );

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 1)
    .expect(oldContainerModel.exists)
    .eql(true)
    .expect(oldNestedModelRow.exists)
    .eql(true)
    .expect(oldNestedPropertyRow.exists)
    .eql(true)
    .expect(newNestedPropertyRow.exists)
    .eql(true)
    .expect(newNestedPropertyRow.textContent)
    .contains('date-time');
});

test('Add new fields to a 2x nested model', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  await addNewFieldsToNestedModel(t);

  // Save the nested model
  await t.click(utils.saveModelBtn);
  // Save the container model
  await t.click(utils.saveModelBtn);

  const oldContainerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const oldNestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const oldNestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const newNestedPropertyRow = Selector('tr.treegrid-body-row').withText('newNestedProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 1)
    .expect(oldContainerModel.exists)
    .eql(true)
    .expect(oldNestedModelRow.exists)
    .eql(true)
    .expect(oldNestedPropertyRow.exists)
    .eql(true)
    .expect(newNestedPropertyRow.exists)
    .eql(true)
    .expect(newNestedPropertyRow.textContent)
    .contains('date-time');
});

test('Cancel edits on a field in the parent model', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  // Setup to add a new row to the parent model
  const nestedFieldRowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = nestedFieldRowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  await t.click(utils.addNewFieldNestedBtn);

  await t.typeText(utils.inputFieldName, 'newProperty', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('Integer'));

  await t.click(utils.saveFieldBtn);

  await t.click(utils.saveModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const addedContainerPropertyRow = Selector('tr.treegrid-body-row').withText('newProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 1)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(true)
    .expect(addedContainerPropertyRow.exists)
    .eql(true)
    .expect(addedContainerPropertyRow.textContent)
    .contains('integer');

  await t.click(editFieldBtn);
  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('newProperty');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.typeText(utils.inputFieldName, 'editedNewProperty', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));

  await t.click(utils.saveFieldBtn);

  await t.click(utils.cancelModelBtn);

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 1)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(true)
    .expect(addedContainerPropertyRow.exists)
    .eql(true)
    .expect(addedContainerPropertyRow.textContent)
    .contains('integer');
});

test('Delete a nested model (from the parent model)', async t => {
  await addNewNestedModelAtRoot(t);
  // To prevent having zero rows in the parent model after deletion
  await addNewFieldsToParentModel(t);
  // Save the container model
  await t.click(utils.saveModelBtn);

  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  const deleteModelBtn = Selector('#modelModalWindow').find('#deleteModelBtn');
  await t.click(deleteModelBtn);

  await t.expect(utils.modelName.value).eql('testModel');
  await t.expect(nestedModelRowToEdit.exists).eql(false);

  await t.click(utils.saveModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount - 2)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(false);
});

test('Edit a nested field after deleting a nested model (from the parent model)', async t => {
  await addNewNestedModelAtRoot(t);
  // To prevent having zero rows in the parent model after deletion
  await addNewFieldsToParentModel(t);
  // Save the container model
  await t.click(utils.saveModelBtn);

  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  const deleteModelBtn = Selector('#modelModalWindow').find('#deleteModelBtn');
  await t.click(deleteModelBtn);

  await t.expect(utils.modelName.value).eql('testModel');
  await t.expect(nestedModelRowToEdit.exists).eql(false);

  await t.click(utils.saveModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount - 2)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(false);

  // Edit from parent model
  await t.click(editFieldBtn);
  const nestedFieldToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('newTestProperty');
  const nestedFieldEditBtn = nestedFieldToEdit.find('.edit-action');
  await t.click(nestedFieldEditBtn);

  await t.expect(utils.inputFieldName.value).eql('newTestProperty');
  await t.expect(utils.inputFieldExample.value).eql('2019-01-01T18:14:29Z');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('DateTime');

  await t.click(utils.cancelFieldBtn);

  await t.click(utils.cancelModelBtn);

  // Edit from root grid
  const rootFieldToEdit = Selector('tr.treegrid-body-row').withText('newTestProperty');
  const rootFieldEditBtn = rootFieldToEdit.find('.edit-action');
  await t.click(rootFieldEditBtn);

  await t.expect(utils.inputFieldName.value).eql('newTestProperty');
  await t.expect(utils.inputFieldExample.value).eql('2019-01-01T18:14:29Z');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('DateTime');
});

test('Delete a nested field (from the parent model)', async t => {
  await addNewNestedModelAtRoot(t);
  await addNewFieldsToNestedModel(t);

  // Save the nested model
  await t.click(utils.saveModelBtn);
  // Save the container model
  await t.click(utils.saveModelBtn);

  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedPropertyRowToDelete = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('newNestedProperty');
  const nestedPropertyEditFieldBtn = nestedPropertyRowToDelete.find('.edit-action');
  await t.click(nestedPropertyEditFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('newNestedProperty');
  await t.expect(utils.inputFieldExample.value).eql('2019-01-01T18:14:29Z');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('DateTime');

  await t.click(utils.deleteFieldBtn);

  await t.click(utils.saveModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const nestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const deletedPropertyRow = Selector('tr.treegrid-body-row').withText('newNestedProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount - 1)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRow.exists)
    .eql(true)
    .expect(deletedPropertyRow.exists)
    .eql(false);
});

test('Delete a nested field (from the model)', async t => {
  await addNewNestedModelAtRoot(t);
  await addNewFieldsToNestedModel(t);

  // Save the nested model
  await t.click(utils.saveModelBtn);
  // Save the container model
  await t.click(utils.saveModelBtn);

  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  const nestedPropertyRowToDelete = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('newNestedProperty');
  const nestedPropertyEditFieldBtn = nestedPropertyRowToDelete.find('.edit-action');
  await t.click(nestedPropertyEditFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('newNestedProperty');
  await t.expect(utils.inputFieldExample.value).eql('2019-01-01T18:14:29Z');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('DateTime');

  await t.click(utils.deleteFieldBtn);

  // Save the nested model
  await t.click(utils.saveModelBtn);

  // The deleted row does not exist in the container model
  await t.expect(nestedPropertyRowToDelete.exists).eql(false);

  // Save the container model
  await t.click(utils.saveModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const nestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const deletedPropertyRow = Selector('tr.treegrid-body-row').withText('newNestedProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount - 1)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRow.exists)
    .eql(true)
    .expect(deletedPropertyRow.exists)
    .eql(false);
});

test('Cancel edits to a nested model (rows added but don’t want to save them)', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;
  await addNewFieldsToNestedModel(t);

  await t.click(utils.cancelModelBtn);

  const canceledRow = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('newNestedProperty');
  await t.expect(canceledRow.exists).eql(false);

  await t.click(utils.saveModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const nestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const canceledPropertyRow = Selector('tr.treegrid-body-row').withText('newNestedProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRow.exists)
    .eql(true)
    .expect(canceledPropertyRow.exists)
    .eql(false);
});

test('Cancel edits to a nested field in the model', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  const nestedFieldRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('testProperty');
  const nestedFieldEditFieldBtn = nestedFieldRowToEdit.find('.edit-action');
  await t.click(nestedFieldEditFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  await t.typeText(utils.inputFieldName, 'nestedPropertyEdited', { replace: true });
  await t.click(utils.inputType).click(Selector('li').withText('DateTime'));
  await t.typeText(utils.inputFieldExample, 'nestedPropertyExample', { replace: true });

  await t.click(utils.saveFieldBtn);

  await t.click(utils.cancelModelBtn);

  const canceledRow = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedPropertyEdited');
  await t.expect(canceledRow.exists).eql(false);

  await t.click(utils.saveModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const nestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const canceledPropertyRow = Selector('tr.treegrid-body-row').withText('nestedPropertyEdited');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRow.exists)
    .eql(true)
    .expect(canceledPropertyRow.exists)
    .eql(false);
});

test('Cancel add new model to an already nested model', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  await t.click(utils.addNewFieldNestedBtn);
  await t.typeText(utils.inputFieldName, 'newModel');
  await t.click(utils.inputType).click(Selector('li').withText('Define a new model'));

  await t.expect(utils.modelName.value).eql('newModel');

  await t.click(utils.cancelModelBtn);

  await t.click(utils.cancelModelBtn);

  await t.click(utils.cancelModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const nestedPropertyRow = Selector('tr.treegrid-body-row').withText('testProperty');
  const canceledModelRow = Selector('tr.treegrid-body-row').withText('newModel');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRow.exists)
    .eql(true)
    .expect(canceledModelRow.exists)
    .eql(false);
});

test('Change the type of a child field to a model', async t => {
  await addNewNestedModelAtRoot(t);
  const initialRowCount = await Selector('tr.treegrid-body-row').count;

  const rowToEdit = Selector('tr.treegrid-body-row').withText('testModel');
  const editFieldBtn = rowToEdit.find('.edit-action');
  await t.click(editFieldBtn);

  await t.expect(utils.modelName.value).eql('testModel');

  const nestedModelRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('nestedModel');
  const nestedModelEditFieldBtn = nestedModelRowToEdit.find('.edit-action');
  await t.click(nestedModelEditFieldBtn);

  await t.expect(utils.modelName.value).eql('nestedModel');

  const nestedFieldRowToEdit = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('testProperty');
  const nestedFieldEditFieldBtn = nestedFieldRowToEdit.find('.edit-action');
  await t.click(nestedFieldEditFieldBtn);

  await t.expect(utils.inputFieldName.value).eql('testProperty');
  await t.expect(utils.inputFieldExample.value).eql('123');
  await t.expect(utils.inputType.getVue(({ props }) => props.value.displayName)).eql('Integer');

  // Change the type to "object"
  await t.click(utils.inputType).click(Selector('li').withText('Define a new model'));

  // Add a field to the newly edited property model
  await t.click(utils.addNewFieldNestedBtn);
  await t.typeText(utils.inputFieldName, '3xNestedProperty');
  await t.click(utils.inputType).click(Selector('li').withText('Integer'));
  await t.click(utils.saveFieldBtn);

  // Save the newly edited property model
  await t.click(utils.saveModelBtn);

  const newlyAddedRowToEditedModel = Selector('#nestedContractGrid')
    .find('tr.treegrid-body-row')
    .withText('3xNestedProperty');
  await t.expect(newlyAddedRowToEditedModel.exists).eql(true);

  // Save the nested model
  await t.click(utils.saveModelBtn);

  await t.expect(newlyAddedRowToEditedModel.exists).eql(true);
  // Save the container model
  await t.click(utils.saveModelBtn);

  const containerModel = Selector('tr.treegrid-body-row').withText('testModel');
  const nestedModelRow = Selector('tr.treegrid-body-row').withText('nestedModel');
  const nestedPropertyRowEdited = Selector('tr.treegrid-body-row').withText('testProperty');
  const new3xNestedPropertyRow = Selector('tr.treegrid-body-row').withText('3xNestedProperty');

  await t
    .expect(Selector('tr.treegrid-body-row').count)
    .eql(initialRowCount + 1)
    .expect(containerModel.exists)
    .eql(true)
    .expect(nestedModelRow.exists)
    .eql(true)
    .expect(nestedPropertyRowEdited.exists)
    .eql(true)
    .expect(nestedPropertyRowEdited.textContent)
    .contains('object')
    .expect(new3xNestedPropertyRow.exists)
    .eql(true);
});
