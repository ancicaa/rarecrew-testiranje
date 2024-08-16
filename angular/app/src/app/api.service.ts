import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = `${environment.api}${environment.key}`;

  constructor(private http: HttpClient) { }

  getEmployees(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }
  
}
