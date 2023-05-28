const btnSubmit = document.querySelector('.btn__submit');

const btnsWrapper = document.querySelector('.modal__btns-wrapper');
const positionSelect = document.querySelector('.pos-select');
const initialPosition = positionSelect.value;

const salaryInput = document.querySelector('.num-salary');

const inputsContainer = document.querySelector('.empl_form-wrapper');

const handleElementsChange = function (e) {
  if (
    !(
      e.target.classList.contains('pos-select') ||
      e.target.classList.contains('num-salary')
    )
  )
    return;

  console.log(salaryInput.value);
  console.log(salaryInput.defaultValue);
  if (
    Number.parseFloat(salaryInput.value) !==
      Number.parseFloat(salaryInput.defaultValue) ||
    positionSelect.value !== initialPosition
  ) {
    btnsWrapper.style.justifyContent = 'space-between';
    btnSubmit.classList.remove('hidden');
    return;
  }

  btnSubmit.classList.add('hidden');
  btnsWrapper.style.justifyContent = 'flex-end';
};

inputsContainer.addEventListener('change', handleElementsChange);
inputsContainer.addEventListener('input', handleElementsChange);
