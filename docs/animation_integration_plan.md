# Flutter Animation Integration Plan (GSAP Concepts to Native Flutter)

This plan details how to implement the gamified animations inspired by GSAP into KIDIO's native Flutter code using the **`flutter_animate`** package and native Flutter controllers for maximum fluid performance (60/120 FPS).

---

## 1. Flashcards & Vocabulary (`vocab_learn_screen.dart`)

### A. 3D Card Flip (Lật thẻ 3D)
* **GSAP concept**: `rotationY` and `transformPerspective`.
* **Flutter implementation**: Use `GestureDetector` paired with `AnimatedBuilder` and `Matrix4.identity()..setEntry(3, 2, 0.002)..rotateY(angle)`.
* **Edge Case - Mirror Artifact**: When the card rotates past 90 degrees ($\pi/2$), rendering the back card directly will display it mirrored.
* **The Fix**: When rendering the back card, apply an additional `rotateY(pi)` transform so the text reads normally.
* **Optimized Layout Template**:
  ```dart
  AnimatedBuilder(
    animation: _flipAnimation,
    builder: (context, child) {
      final angle = _flipAnimation.value * pi;
      final isBack = angle > pi / 2;

      return Transform(
        transform: Matrix4.identity()
          ..setEntry(3, 2, 0.002) // Perspective depth
          ..rotateY(angle),
        alignment: Alignment.center,
        child: isBack
            ? Transform(
                alignment: Alignment.center,
                transform: Matrix4.identity()..rotateY(pi), // Un-mirror the back side
                child: CardBack(),
              )
            : CardFront(),
      );
    },
  );
  ```

### B. Swipe to Classify (Vuốt phân loại)
* **GSAP concept**: `Draggable` plugin with screen boundary bounds.
* **Flutter implementation**: Wrap the card in a `GestureDetector` or custom `Draggable` widget. Calculate horizontal drag offset `xOffset`.
* **Animations**:
  * Rotate slightly based on offset (`xOffset / 15`).
  * On release: if offset exceeds threshold (e.g. 120 pixels), slide card completely off-screen with `Curves.easeOutBack` and paint a green (known) or red (unknown) border. Otherwise, spring back to the center using an elastic curve.

---

## 2. Gamification & Streak Celebration

### A. Confetti & Star Burst (Hiệu ứng pháo giấy/ngôi sao)
* **GSAP concept**: Staggered physics2D particles.
* **Flutter implementation**: Keep the particle count under 15 items to avoid overhead, or trigger them inside an Overlay. Overlay widgets should use `.warmUp()` if using `flutter_animate` to prevent jank on first run.

### B. Streaks Fire Level Up (Ngọn lửa bùng cháy)
* **GSAP concept**: Scale up with `Elastic.easeOut`.
* **Flutter implementation**: Animate the flame icon on the main header:
  ```dart
  Icon(Icons.local_fire_department)
    .animate()
    .scale(
      begin: const Offset(1.0, 1.0),
      end: const Offset(1.3, 1.3),
      duration: 500.ms,
      curve: Curves.elasticOut,
    )
    .then()
    .scale(end: const Offset(1.0, 1.0), duration: 200.ms);
  ```

### C. Progress Bar Elastic Catch-up (Thanh tiến trình đàn hồi)
* **GSAP concept**: Ease width update with a spring/bounce ease.
* **Flutter implementation**: To avoid expensive layout recalculations caused by `FractionallySizedBox` sizing changes, use a `LayoutBuilder` to compute direct pixel widths and animate the container's width property:
  ```dart
  LayoutBuilder(
    builder: (context, constraints) {
      final maxWidth = constraints.maxWidth;
      return TweenAnimationBuilder<double>(
        tween: Tween<double>(begin: oldProgress, end: newProgress),
        duration: 600.ms,
        curve: Curves.easeOutBack,
        builder: (context, progress, child) {
          return Container(
            width: maxWidth * progress,
            // ... styling decoration ...
          );
        },
      );
    },
  )
  ```

---

## 3. Quizzes & Activities

### A. Staggered List Entrance (Hiện đáp án tuần tự)
* **GSAP concept**: `stagger: 0.1` on list items.
* **Flutter implementation**: Ensure the stagger timeline executes perfectly without lazy-loading scroll interruptions by using fixed `Column` or `Wrap` widgets rather than lazy `ListView.builder`:
  ```dart
  optionCard
    .animate()
    .fadeIn(duration: 300.ms)
    .slideY(begin: 0.2, end: 0.0, curve: Curves.easeOutBack, duration: 400.ms)
    .delay(Duration(milliseconds: 100 * index));
  ```

### B. Wrong Answer Shake (Rung lắc khi chọn sai)
* **GSAP concept**: Fast translation back and forth on X axis.
* **Flutter implementation**: Already implemented using `flutter_animate`'s `.shake(hz: 6, duration: 500.ms)`.

---

## 4. Mascot Interactions (Panda Mascot)

### A. Breathing Idle Animation (Linh vật thở nhẹ)
* **GSAP concept**: Infinite scaleY yoyo.
* **Flutter implementation**:
  ```dart
  Text('🐼', style: TextStyle(fontSize: 48))
    .animate(onPlay: (controller) => controller.repeat(reverse: true))
    .scaleY(begin: 1.0, end: 1.06, duration: 2.seconds, curve: Curves.easeInOut);
  ```

### B. Reaction Animations (Vui mừng / Tiếc nuối)
* **GSAP concept**: Jump up on success, shake/rotate on error.
* **Flutter implementation**:
  * **Success/Correct Answer**: Trigger jump:
    ```dart
    pandaWidget
      .animate()
      .slideY(begin: 0.0, end: -0.3, curve: Curves.easeOutQuad, duration: 250.ms)
      .then()
      .slideY(begin: -0.3, end: 0.0, curve: Curves.bounceOut, duration: 350.ms);
    ```
  * **Wrong Answer**: Trigger scale-down and tilt:
    ```dart
    pandaWidget
      .animate()
      .rotate(begin: 0.0, end: -0.05, duration: 150.ms)
      .then()
      .rotate(begin: -0.05, end: 0.0, duration: 150.ms);
    ```

---

## 5. Refined Phased Execution Timeline

```
┌────────────────────────────────────────────────────────┐
│ PHASE 1: Core Layout Mechanics                         │
│ ∙ Implement 3D Card Flip & Swipe bounds (Mirror fix)   │
│ ∙ Refactor Progress Bars using TweenAnimationBuilder   │
│   with LayoutBuilder absolute constraint widths        │
└───────────────────────────┬────────────────────────────┘
                            ▼
┌────────────────────────────────────────────────────────┐
│ PHASE 2: Feedback & Game Feel                          │
│ ∙ Add Staggered Grid/List entrances to quiz screens    │
│   (Column/Wrap static mounts)                          │
│ ∙ Standardize Wrong Answer Shake effects               │
└───────────────────────────┬────────────────────────────┘
                            ▼
┌────────────────────────────────────────────────────────┐
│ PHASE 3: Mascot Ambient Animation & Polish             │
│ ∙ Integrate Mascot breathing loops (Infinite Repeat)   │
│ ∙ Hook up Success/Failure reaction triggers            │
└────────────────────────────────────────────────────────┘
```
