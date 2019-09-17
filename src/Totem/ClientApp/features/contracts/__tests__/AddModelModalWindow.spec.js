import { mount, config } from '@vue/test-utils';
import sinon from 'sinon';
import AddModelModalWindow from '../AddModelModalWindow.vue';
import JavascriptTransitionStub from './stubs/JavascriptTransitionStub';

config.stubs.transition = JavascriptTransitionStub;

describe('AddModelModalWindow.vue', () => {
  const wrapper = mount(AddModelModalWindow, {
    propsData: {
      title: 'Test Add New Field',
      fieldName: 'Test Field',
      modalRows: [
        {
          name: 'Test Property',
          rowId: 1,
          type: 'string'
        }
      ]
    }
  });
  const showFieldWindowStub = sinon.stub();
  const enterStub = sinon.stub();
  const deleteModelStub = sinon.stub();
  const closeStub = sinon.stub();
  const saveModelStub = sinon.stub();

  wrapper.setMethods({
    enter: enterStub,
    showFieldWindow: showFieldWindowStub
  });

  wrapper.setData({
    successBtn: {
      id: 'successBtnId',
      text: 'Ok',
      clicked: saveModelStub
    },
    cancelBtn: {
      id: 'cancelBtnId',
      text: 'Cancel',
      clicked: closeStub
    },
    deleteBtn: {
      id: 'deleteBtnId',
      text: 'Delete',
      clicked: deleteModelStub
    },
    isEditModal: true
  });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBe(true);
  });

  it('calls deleteModel on delete button click', () => {
    const deleteButton = wrapper.find('#deleteBtnId');
    deleteButton.trigger('click');
    expect(deleteModelStub.called).toBe(true);
  });

  it('calls saveModel on save button click', () => {
    const saveButton = wrapper.find('#successBtnId');
    saveButton.trigger('click');
    expect(saveModelStub.called).toBe(true);
  });

  it('calls close on close button click', () => {
    const closeButton = wrapper.find('#cancelBtnId');
    closeButton.trigger('click');
    expect(closeStub.called).toBe(true);
  });

  it('calls enter() on modal enter event', () => {
    const transition = wrapper.find({ name: 'JavascriptTransitionStub' });
    transition.vm.triggerEnterHooks();
    expect(enterStub.called).toBe(true);
  });

  it('calls addNewField on button click', () => {
    const button = wrapper.find('#addNewFieldBtn');
    button.trigger('click');
    expect(showFieldWindowStub.called).toBe(true);
  });
});
