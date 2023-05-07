const modal = document.querySelector('.modal');
const overlay = document.querySelector('.overlay');
const btnCloseModal = document.querySelector('.modal__close-btn');
const btnOpenModal = document.querySelector('.nav__login');
const btnLogin = document.querySelector('.btn-login');

if (btnCloseModal && btnOpenModal) {
  btnCloseModal.addEventListener('click', function () {
    modal.classList.add('hidden');
    overlay.classList.add('hidden');
  });

  btnOpenModal.addEventListener('click', function () {
    modal.classList.remove('hidden');
    overlay.classList.remove('hidden');
  });
}
