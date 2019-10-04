import VueSelector from 'testcafe-vue-selectors';
import { Selector } from 'testcafe';

// Input fields and buttons on the AddNewFieldModal
export const inputFieldName = Selector('#propertyName');
export const inputType = VueSelector('ref:propertyType');
export const isArrayCheckbox = Selector('#isArray');
export const isObjectArrayCheckbox = Selector('#isObjectArray');
export const inputFieldExample = Selector('#propertyExample');

export const addNewFieldBtn = Selector('#addNewFieldBtn');
export const saveFieldBtn = Selector('#saveFieldBtn');
export const deleteFieldBtn = Selector('#deleteFieldBtn');
export const cancelFieldBtn = Selector('#modalCancelButton');

// Input fields and buttons on the AddNewModelModal
export const modelName = Selector('#modelName');
export const saveModelBtn = Selector('#saveModelBtn');
export const cancelModelBtn = Selector('#modelModalWindow').find('#cancelBtn');
export const addNewFieldNestedBtn = Selector('#nestedContractGrid').find('#addNewFieldBtn');

// Import Contract buttons
export const importContractBtn = Selector('#importContractFromMessageBtn');
export const importBtn = Selector('#importContract');
export const importTextArea = Selector('#import-message');

// Fixture Setup: Login and Go to Edit
export async function loginAndNavigateToEditContract(t) {
  const goToLoginButton = Selector('#loginBtn');
  await t.click(goToLoginButton);

  const emailInput = Selector('#Input_Email');
  const emailPassword = Selector('#Input_Password');
  await t.typeText(emailInput, 'testuser@headspring.com');
  await t.typeText(emailPassword, '1234@TestUser');

  const loginButton = Selector('#loginSubmit');
  await t.click(loginButton);

  const contractEditButton = Selector('.edit-contract').nth(1);
  await t.click(contractEditButton);
}

// Fixture Teardown: Logout
export async function logOut(t) {
  const logoutButton = Selector('#logoutBtn');
  await t.click(logoutButton);
}
