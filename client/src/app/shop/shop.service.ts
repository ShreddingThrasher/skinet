import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Pagination } from '../shared/models/pagination';
import { Product } from '../shared/models/product';
import { Brand } from '../shared/models/brand';
import { Type } from '../shared/models/type';

@Injectable({
  providedIn: 'root'
})
export class ShopService {

  baseUrl = "https://localhost:5001/api/";

  constructor(private http: HttpClient) { }

  getProducts(brandId?: number, typeId?: number){

    let params = new HttpParams();

    if(brandId) params.append('brandId', brandId);
    if(typeId) params.append('typeId', typeId);

    return this.http.get<Pagination<Product>>(this.baseUrl + 'products?pageSize=50');
  }

  getBrands() {
    return this.http.get<Brand[]>(this.baseUrl + 'products/brands');
  }

  getTypes() {
    return this.http.get<Type[]>(this.baseUrl + 'products/types');
  }
}
