import { mount } from '@vue/test-utils';
import sinon from 'sinon';
import ContractGrid from '../ContractGrid.vue';

describe('ContractGrid.vue', () => {
  const wrapper = mount(ContractGrid, {
    propsData: {
      rows: [
        {
          name: 'Test Property',
          rowId: 1,
          type: 'string'
        }
      ],
      isEllipsisMenuVisible: true
    }
  });

  const handleEditClickStub = sinon.stub();
  wrapper.setMethods({
    handleEditClick: handleEditClickStub
  });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBe(true);
  });

  it('calls handleEditClick method on edit-icon click', () => {
    const editButton = wrapper.find('.edit-action');
    editButton.trigger('click');
    expect(handleEditClickStub.called).toBe(true);
  });

  it('hides EllipsisMenu if isEllipsisMenuVisble set to false', () => {
    expect(wrapper.find('.ellipsis-menu').exists()).toBe(true);
    wrapper.setProps({
      isEllipsisMenuVisible: false
    });
    expect(wrapper.find('.ellipsis-menu').exists()).toBe(false);
  });
});
