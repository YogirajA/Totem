import { TransitionStub } from '@vue/test-utils';

export default {
  name: 'JavascriptTransitionStub',
  extends: TransitionStub,
  methods: {
    triggerEnterHooks() {
      this.$emit('beforeEnter');
      this.$emit('enter');
      this.$emit('afterEnter');
    },
    triggerLeaveHooks() {
      this.$emit('beforeLeave');
      this.$emit('leave');
      this.$emit('afterLeave');
    }
  }
};
