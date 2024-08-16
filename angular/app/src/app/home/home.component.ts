import { Component, OnInit } from '@angular/core';
import { ApiService } from '../api.service';


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  employees: any[] = []

  constructor(private api: ApiService) { }

  ngOnInit(): void {
    this.getEmployees()
  }

  getEmployees() {
    this.api.getEmployees().subscribe(data => {
      const employeeMap: { [key: string]: number } = {};

      data.forEach(entry => {
        if (!entry.EmployeeName) {
          return
        }
        const employeeName = entry.EmployeeName;
        const hours = this.calculateWorkHours(entry.StarTimeUtc,entry.EndTimeUtc);

        if (employeeMap[employeeName]) {
          employeeMap[employeeName] += hours;
        } else {
          employeeMap[employeeName] = hours;
        }
      });

      this.employees = Object.entries(employeeMap).map(([name, totalHours]) => ({
        employeeName: name,
        totalHours
    }));
    

      this.employees.sort((a, b) => b.totalHours - a.totalHours);
      console.log('Employee names:', this.employees);
    });
  }

  calculateWorkHours(startTime: string, endTime: string ){
    return  (new Date(endTime).getTime() - new Date(startTime).getTime())/(1000 * 60 * 60)
  }
}




