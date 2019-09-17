import { mount } from '@vue/test-utils';
import sinon from 'sinon';
import EditContractModalWindow from '../EditContractModalWindow.vue';

describe('EditContractModalWindow.vue', () => {
  const wrapper = mount(EditContractModalWindow, {
    propsData: {
      title: 'Test Edit Contract Manually',
      contractString: JSON.stringify({
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
  const updateDataStub = sinon.stub();
  const closeStub = sinon.stub();

  wrapper.setMethods({
    updateData: updateDataStub,
    close: closeStub
  });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBe(true);
  });

  it('calls updateData on save button click', () => {
    const updateButton = wrapper.find('#updateContract');
    updateButton.trigger('click');
    expect(updateDataStub.called).toBe(true);
  });

  it('calls close on close button click', () => {
    const closeButton = wrapper.find('#cancelBtn');
    closeButton.trigger('click');
    expect(closeStub.called).toBe(true);
  });
});
