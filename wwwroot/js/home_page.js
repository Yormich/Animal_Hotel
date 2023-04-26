class Slider {
  #slides;
  curSlide = 0;
  #btnLeft;
  #btnRight;
  #maxSLides;
  #dotsContainer;
  constructor() {
    //initiate elements
    this.#slides = document.querySelectorAll('.slide');
    this.#maxSLides = this.#slides.length;
    this.#btnRight = document.querySelector('.slider__btn-right');
    this.#btnLeft = document.querySelector('.slider__btn-left');
    this.#dotsContainer = document.querySelector('.dots');

    //add event listeners
    this.#btnRight.addEventListener('click', this.nextSlide.bind(this));
    this.#btnLeft.addEventListener('click', this.prevSlide.bind(this));
    this.#dotsContainer.addEventListener('click', this.dotClicked.bind(this));

    //init slider
    this.goToSlide(this.curSlide);
    this.activateDot(this.curSlide);
  }

  goToSlide(slide) {
    this.#slides.forEach(
      (s, i) => (s.style.transform = `translateX(${100 * (i - slide)}%)`)
    );
  }

  activateDot(slide) {
    const dots = document.querySelectorAll('.dots__dot');
    dots.forEach(dot => dot.classList.remove('dots__dot-active'));

    document
      .querySelector(`.dots__dot[data-slide="${slide}"]`)
      .classList.add('dots__dot-active');
  }

  nextSlide() {
    this.curSlide =
      this.curSlide === this.#maxSLides - 1 ? 0 : this.curSlide + 1;

    this.goToSlide(this.curSlide);
    this.activateDot(this.curSlide);
  }
  prevSlide() {
    this.curSlide =
      this.curSlide === 0 ? this.#maxSLides - 1 : this.curSlide - 1;

    this.goToSlide(this.curSlide);
    this.activateDot(this.curSlide);
  }

  dotClicked(e) {
    if (e.target.classList.contains('dots__dot')) {
      const { slide } = e.target.dataset;
      this.goToSlide(slide);
      this.activateDot(slide);
    }
  }
}

let slider;

document.addEventListener('DOMContentLoaded', function () {
  slider = new Slider();
});
