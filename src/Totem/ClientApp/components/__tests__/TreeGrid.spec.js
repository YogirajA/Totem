import { shallowMount } from '@vue/test-utils';
import TreeGrid from '../TreeGrid.vue';

describe('TreeGrid.vue', () => {
  const wrapper = shallowMount(TreeGrid);

  test('is a Vue instance', () => {
    expect(wrapper.isVueInstance()).toBe(true);
  });
});
