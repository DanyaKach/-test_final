import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  vus: 50,
  duration: '30s',
};

const baseUrl = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  const stationId = 1;
  const payload = JSON.stringify({
    temperature: Math.random() * 80 - 20,
    humidity: Math.random() * 100,
    windSpeedKmh: Math.random() * 140,
    pressure: 980 + Math.random() * 60,
    recordedAt: new Date().toISOString()
  });

  const params = { headers: { 'Content-Type': 'application/json' } };
  const res = http.post(`${baseUrl}/api/stations/${stationId}/readings`, payload, params);

  check(res, {
    'status is 201 or 400': r => r.status === 201 || r.status === 400,
  });

  sleep(0.1);
}
