import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { environment } from '@environments/environment';
import { User } from '@app/_models';

@Injectable({ providedIn: 'root' })
export class UserService {
  constructor(private http: HttpClient) { }

  getAll() {
    return this.http.get<User[]>(`${environment.apiUrl}/users`);
  }

  getById(id: string) {
    return this.http.get<User>(`${environment.apiUrl}/users/${id}`);
  }

  // create(params: any) {
  //   return this.http.post(environment.apiUrl, params);
  // }
  create(id: string, params: any) {
    return this.http.post(`${environment.apiUrl}/users`, params);
  }
  update(id: string, params: any) {
    return this.http.put(`${environment.apiUrl}/users/${id}`, params);
  }

  delete(id: string) {
    return this.http.delete(`${environment.apiUrl}/users/${id}`);
  }
}
