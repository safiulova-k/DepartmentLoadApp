import http from 'k6/http';
import { check, sleep, group } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'https://localhost:7014';
const VUS = Number(__ENV.VUS || 10);

export const options = {
    vus: VUS,
    duration: '1m',
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<5000'],
        checks: ['rate>0.95']
    }
};

function requestBackend(path, name) {
    const response = http.get(`${BASE_URL}${path}`);

    check(response, {
        [`${name}: успешный HTTP-ответ`]: (r) => r.status >= 200 && r.status < 400,
        [`${name}: время ответа меньше 5000 мс`]: (r) => r.timings.duration < 5000
    });

    sleep(1);
}

export default function () {
    group('Главный модуль', function () {
        requestBackend('/', 'Главная страница модуля');
    });

    group('Контингент', function () {
        requestBackend('/Contingent', 'Получение контингента');
    });

    group('Нормы времени', function () {
        requestBackend('/NormTime', 'Получение норм времени');
    });

    group('Расчёт нагрузки', function () {
        requestBackend('/WorkloadCalculation', 'Получение расчётной таблицы');
    });

    group('Распределение нагрузки', function () {
        requestBackend('/WorkloadDistribution', 'Получение распределения нагрузки');
    });

    group('Индивидуальные планы', function () {
        requestBackend('/IndividualPlans', 'Получение индивидуальных планов');
    });
}