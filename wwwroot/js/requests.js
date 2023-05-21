const initiateEventHandlers = function () {
  const modal = document.querySelector('.modal');
  const overlay = document.querySelector('.overlay');
  const modalForm = document.querySelector('.rmodal__form');
  const modalTextArea = document.querySelector('.form__input-textarea');

  const requestsContainer = document.querySelector('.requests_container');
  const newRequestBtn = document.querySelector('.new_request-btn');
  const closeModalBtn = document.querySelector('.modal__close-btn');
  const btnSubmitData = document.querySelector('.btn__submit');

  const showModal = function () {
    modal.classList.remove('hidden');
    overlay.classList.remove('hidden');
  };

  const closeModal = function () {
    modal.classList.add('hidden');
    overlay.classList.add('hidden');

    document.querySelector('.new__request-span').classList.add('hidden');
    btnSubmitData.classList.add('hidden');
    modalTextArea.readOnly = true;
  };

  const bindModal = function (modal, action, controller = 'EmployeeCommon') {
    const pageIndex = document
      .querySelector('.page_index-header')
      .textContent.trim();
    modal.setAttribute(
      'action',
      `/${controller}/${action}?pageIndex=${pageIndex}`
    );
  };

  if (sessionStorage.length != 0) {
    (function () {
      const actionValue = sessionStorage.getItem('modal__action');

      if (!actionValue) return;

      sessionStorage.removeItem('modal__action');
      const [action, controller] = actionValue.split(' ');
      modalTextArea.removeAttribute('readonly');
      document.querySelector('.request__id-span').classList.remove('hidden');
      btnSubmitData.classList.remove('hidden');
      bindModal(modalForm, action, controller);
    })();
  }

  requestsContainer.addEventListener('click', function (e) {
    const clicked = e.target;
    if (clicked.classList.contains('request__edit-btn')) {
      sessionStorage.setItem(
        'modal__action',
        'UpdateEmployeeRequest EmployeeCommon'
      );
    }
  });

  //   newRequestBtn.addEventListener('click', function () {
  //     showModal();
  //     bindModal(modalForm, 'AddNewRequest');

  //     const writerName = document.querySelector('#user__name').textContent;
  //     const status = document.querySelector(
  //       '.request__default-status'
  //     ).textContent;
  //     btnSubmitData.classList.remove('hidden');
  //     document.querySelector('.new__request-span').classList.remove('hidden');
  //     document.querySelector('#writer_name').value = writerName;
  //     document.querySelector('#status').value = status;
  //     modalTextArea.textContent = '';
  //     modalTextArea.removeAttribute('readonly');
  //   });

  closeModalBtn.addEventListener('click', closeModal);
};

document.addEventListener('DOMContentLoaded', function () {
  initiateEventHandlers();
});
