const init = function (
  formSelector,
  inputSelector = 'form_input',
  editBtnsSelector = 'edit__info-btn',
  seeBtnsSelector = 'see__info-btn'
) {
  //variables
  const formContainer = document.querySelector(`.${formSelector}`);
  const inputElements = document.querySelectorAll(`.${inputSelector}`);
  const listItems = document.querySelectorAll('.user__action-item');
  const changePasswordBtn = document.querySelector('.change-passwd-btn-cont');
  const submitDataBtn = document.querySelector('.submit__data-btn');
  const fileInput = document.getElementById('photo-input');
  const fileLbl = document.querySelector('.for_input-file');

  //functions

  const enablePasswordInputs = function () {
    document
      .querySelectorAll('.form__hidden')
      .forEach(el => el.classList.remove('form__hidden'));
  };

  const removeInputReadonly = function (editBtn, input) {
    input.removeAttribute('readonly');
    editBtn.classList.add('hidden');
  };

  const makeInputValueVisible = function (seeBtn, input) {
    const isVisible = seeBtn.src.includes('close-eye.png');
    seeBtn.src = isVisible
      ? seeBtn.src.replace('close-eye.png', 'openeye.png')
      : seeBtn.src.replace('openeye.png', 'close-eye.png');
    input.type = isVisible ? 'password' : 'text';
  };

  const checkInputValues = function () {
    for (let i = 0; i < inputElements.length; i++) {
      if (inputElements[i].value != inputElements[i].defaultValue) {
        submitDataBtn.classList.remove('hidden');
        return;
      }

      submitDataBtn.classList.add('hidden');
    }
  };

  listItems[0].classList.add('ml0');

  //event handlers

  document.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') {
      e.preventDefault();
    }
  });

  changePasswordBtn.addEventListener('click', function () {
    changePasswordBtn.classList.add('hidden');

    enablePasswordInputs();
    changePasswordBtn.remove();
  });

  formContainer.addEventListener('click', function (e) {
    const inputEl = e.target.parentElement.parentElement.querySelector(
      `.${inputSelector}`
    );
    if (e.target.classList.contains(editBtnsSelector) && inputEl) {
      removeInputReadonly(e.target, inputEl);
    }

    if (e.target.classList.contains(seeBtnsSelector) && inputEl) {
      makeInputValueVisible(e.target, inputEl);
    }
  });

  fileInput.addEventListener('change', function (e) {
    fileLbl.textContent = `Uploaded ${e.target.files[0].name}`;
    submitDataBtn.classList.remove('hidden');
    e.stopPropagation();
  });

  //   inputElements.forEach(el =>
  //     el.addEventListener('change', function (e) {
  //       console.log(e.target);
  //     })
  //   );

  formContainer.addEventListener('input', checkInputValues);
  formContainer.addEventListener('change', checkInputValues);
};

document.addEventListener('DOMContentLoaded', function () {
  init('form__wrapper');
});
