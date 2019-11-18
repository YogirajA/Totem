import { mount } from '@vue/test-utils';
import sinon from 'sinon';
import BuildContractFromMessageModalWindow from '../BuildContractFromMessageModalWindow.vue';

describe('BuildContractFromMessageModalWindow.vue', () => {
  const wrapper = mount(BuildContractFromMessageModalWindow, {
    propsData: {
      title: 'Test Import Contract'
    }
  });
  const importContractStub = sinon.stub();
  const closeStub = sinon.stub();

  wrapper.setMethods({
    importContract: importContractStub,
    close: closeStub
  });

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBe(true);
  });

  it('calls importContract on import button click', () => {
    const importButton = wrapper.find('#importContract');
    importButton.trigger('click');
    expect(importContractStub.called).toBe(true);
  });

  it('calls close on close button click', () => {
    const closeButton = wrapper.find('#cancelBtn');
    closeButton.trigger('click');
    expect(closeStub.called).toBe(true);
  });
});
