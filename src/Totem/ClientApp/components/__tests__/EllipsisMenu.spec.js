import { mount } from '@vue/test-utils';
import EllipsisMenu from '../EllipsisMenu.vue';

describe('ContractGrid.vue', () => {
  test('is a Vue instance', () => {
    const wrapper = mount(EllipsisMenu);
    expect(wrapper.isVueInstance()).toBe(true);
  });

  it('opens edit contract modal on button click', () => {
    const wrapper = mount(EllipsisMenu);
    const button = wrapper.find('button');
    button.trigger('click');
    expect(wrapper.emitted().showModal).toBeTruthy();
  });
});
