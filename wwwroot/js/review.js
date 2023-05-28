const formContainer = document.querySelector('.client__review-form');
const rangeInput = document.querySelector('.form__input-range');
const rangeDisplay = document.querySelector('.range_input-display');
const rangeSpan = document.querySelector('span[class="review__rating"]');

const submitBtn = document.querySelector('.btn__submit');

const textarea = document.querySelector('.form__input-textarea');

const handleInputChanges = function () {
  if (
    textarea.value !== textarea.textContent ||
    rangeInput.value !== rangeInput.defaultValue
  ) {
    document.querySelector('.manage_btns-wrapper').style.justifyContent =
      'space-between';
    submitBtn.classList.remove('hidden');
    return;
  }
  document.querySelector('.manage_btns-wrapper').style.justifyContent =
    'flex-end';
  submitBtn.classList.add('hidden');
};

rangeInput.addEventListener('input', function (e) {
  rangeSpan.textContent = `${e.target.value}/10`;
  rangeDisplay.textContent = e.target.value;
});

formContainer.addEventListener('input', handleInputChanges);
formContainer.addEventListener('change', handleInputChanges);
