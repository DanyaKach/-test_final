import http from 'k6/http'
import { check, sleep } from 'k6'

export const options = {
  vus: 20,
  duration: '20s',
}

const baseUrl = __ENV.BASE_URL || 'http://localhost:5000'

export default function () {
  const stationId = 1
  const from = encodeURIComponent(new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString())
  const to = encodeURIComponent(new Date().toISOString())
  const res = http.get(`${baseUrl}/api/stations/${stationId}/average?from=${from}&to=${to}`)

  check(res, {
    'status is 200 or 400': r => r.status === 200 || r.status === 400,
  })

  sleep(0.5)
}
