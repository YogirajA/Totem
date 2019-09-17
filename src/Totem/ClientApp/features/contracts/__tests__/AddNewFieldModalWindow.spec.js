import { mount, config } from '@vue/test-utils';
import sinon from 'sinon';
import AddNewFieldModalWindow from '../AddNewFieldModalWindow.vue';
import JavascriptTransitionStub from './stubs/JavascriptTransitionStub';

config.stubs.transition = JavascriptTransitionStub;

describe('AddNewFieldModalWindow.vue', () => {
  const wrapper = mount(AddNewFieldModalWindow, {
    propsData: {
      title: 'Test Add New Field',
      name: 'Test Field',
      options: ['1', '2', '3'],
      id: 'id',
      modifiedContract: JSON.stringify({
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
      })
    }
  });

  const onChangeStub = sinon.stub();
  const onModalShowStub = sinon.stub();
  const onModalHideStub = sinon.stub();
  const deleteFieldStub = sinon.stub();
  const closeStub = sinon.stub();
  const saveFieldStub = sinon.stub();

  wrapper.setMethods({
    onChange: onChangeStub,
    onModalShow: onModalShowStub,
    onModalHide: onModalHideStub
  });

  wrapper.setData({
    successBtn: {
      id: 'successBtnId',
      text: 'Ok',
      clicked: saveFieldStub
    },
    cancelBtn: {
      id: 'cancelBtnId',
      text: 'Cancel',
      clicked: closeStub
    },
    deleteBtn: {
      id: 'deleteBtnId',
      text: 'Delete',
      clicked: deleteFieldStub
    }
  });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBe(true);
  });

  it('calls deleteField on delete button click', () => {
    const deleteButton = wrapper.find('#deleteBtnId');
    deleteButton.trigger('click');
    expect(deleteFieldStub.called).toBe(true);
  });

  it('calls saveField on save button click', () => {
    const saveButton = wrapper.find('#successBtnId');
    saveButton.trigger('click');
    expect(saveFieldStub.called).toBe(true);
  });

  it('calls close on close button click', () => {
    const closeButton = wrapper.find('#cancelBtnId');
    closeButton.trigger('click');
    expect(closeStub.called).toBe(true);
  });

  it('calls onModalShow on modal enter event', () => {
    const transition = wrapper.find({ name: 'JavascriptTransitionStub' });
    transition.vm.triggerEnterHooks();
    expect(onModalShowStub.called).toBe(true);
  });

  it('calls onModalHide on modal leave event', () => {
    const transition = wrapper.find({ name: 'JavascriptTransitionStub' });
    transition.vm.triggerLeaveHooks();
    expect(onModalHideStub.called).toBe(true);
  });

  it('calls onChange on dropdown option change', () => {
    const multiSelect = wrapper.find('.multiselect');
    multiSelect.vm.select(multiSelect.vm.options[1]);
    expect(onChangeStub.called).toBe(true);
    expect(wrapper.vm.fieldType).toBe('2');
  });
});
