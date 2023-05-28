import { Component, OnInit } from '@angular/core';
import { BasketService } from './basket/basket.service';

type NewType = OnInit;

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements NewType {
  title = 'Skinet';

  constructor(private basketService: BasketService) { }

  ngOnInit(): void {
    const basketId = localStorage.getItem('basket_id');

    if(basketId) this.basketService.getBasket(basketId);
  }
}
