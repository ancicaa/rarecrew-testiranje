import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js/auto';
import ChartDataLabels from 'chartjs-plugin-datalabels';


@Component({
  selector: 'app-piechart',
  templateUrl: './piechart.component.html',
  styleUrls: ['./piechart.component.css']
})
export class PiechartComponent implements OnInit {
  @Input() data: any;

  chart?: Chart<'pie', number[], string>;

  ngOnInit() {
    Chart.register(...registerables, ChartDataLabels);
  }


  ngOnChanges(changes: SimpleChanges) {
    if (changes['data'] && this.data) {
      this.createChart(Object.values(this.data));
    }
  }


  generateColors(count: number) {
    const colors = [];
    for (let i = 0; i < count; i++) {
      const r = Math.floor(Math.random() * 256);
      const g = Math.floor(Math.random() * 256);
      const b = Math.floor(Math.random() * 256);
      colors.push(`rgba(${r}, ${g}, ${b}, 0.8)`);
    }
    return colors;
  }


  createChart(employeeData: any[]) {
    if (this.chart) {
      this.chart.destroy();
    }
    const totalHours = employeeData.reduce((sum, employee) => sum + employee.totalHours, 0);
    const percentages = employeeData.map(employee => (employee.totalHours / totalHours) * 100);

    const config: ChartConfiguration<'pie', number[], string> = {
      type: 'pie',
      data: {
        labels: employeeData.map(employee => employee.employeeName),
        datasets: [{
          data: percentages,
          backgroundColor: this.generateColors(employeeData.map(employee => employee.totalHours).length),
          borderWidth: 0
        }]
      },
      options: {
        responsive: true,
        plugins: {
          legend: {
            display: true,
            position: 'top',
            title: {
              display: true
            }
          },
          datalabels: {
            formatter: (value: number) => `${Math.round(value)}%`,
            color: '#fff',
            font: {
              weight: 'bold',
              size: 14
            },
            anchor: 'center',
            align: 'center',
            offset: 0,
          }
        }
      }
    }
    this.chart = new Chart('pieChart', config);
  }


}


