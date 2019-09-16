import { shallowMount } from '@vue/test-utils';
import ModalWindow from '../ModalWindow.vue';

describe('ModalWindow.vue', () => {
  const wrapper = shallowMount(ModalWindow, {
    propsData: {
      title: 'Test Modal',
      successBtn: {
        id: 'successBtnId',
        text: 'Ok',
        clicked: () => {}
      },
      cancelBtn: {
        id: 'cancelBtnId',
        text: 'Cancel',
        clicked: () => {}
      },
      deleteBtn: {
        id: 'deleteBtnId',
        text: 'Delete',
        clicked: () => {}
      },
      isEditing: false
    }
  });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBe(true);
  });

  it('emits success event when successButton clicked', () => {
    wrapper.setProps({
      successBtn: {
        id: 'successBtnId',
        text: 'Success Test',
        clicked: () => {
          wrapper.vm.$emit('success');
        }
      }
    });
    const button = wrapper.find('#successBtnId');
    button.trigger('click');
    expect(wrapper.emitted().success).toBeTruthy();
  });

  it('emits cancel event when cancelButton clicked', () => {
    wrapper.setProps({
      cancelBtn: {
        id: 'cancelBtnId',
        text: 'Cancel Test',
        clicked: () => {
          wrapper.vm.$emit('cancel');
        }
      }
    });
    const button = wrapper.find('#cancelBtnId');
    button.trigger('click');
    expect(wrapper.emitted().cancel).toBeTruthy();
  });

  it('hides delete button when isEditing false', () => {
    expect(wrapper.props().isEditing).toBe(false);
    expect(wrapper.find('#deleteBtnId').isVisible()).toBe(false);
  });

  it('emits delete event when deleteButton clicked', () => {
    wrapper.setProps({
      deleteBtn: {
        id: 'deleteBtnId',
        text: 'Delete Test',
        clicked: () => {
          wrapper.vm.$emit('delete');
        }
      },
      isEditing: true
    });
    const button = wrapper.find('#deleteBtnId');
    button.trigger('click');
    expect(wrapper.props().isEditing).toBe(true);
    expect(wrapper.emitted().delete).toBeTruthy();
  });
});
