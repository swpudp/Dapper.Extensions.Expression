CREATE DATABASE  if not exists "dapper_exp";

\c "dapper_exp";

CREATE TABLE "attachment" (
  "Id" character(36) NOT NULL,
  "OrderId" character(36) NOT NULL,
  "Name" character varying(45) NOT NULL,
  "Extend" character varying(45) NOT NULL,
  "Version" int(11) NOT NULL,
  PRIMARY KEY ("Id")
) ;

CREATE TABLE "buyer" (
  "Id" character(36) NOT NULL,
  "Name" character varying(45) NOT NULL,
  "Type" tinyint(1) NOT NULL,
  "Code" character varying(45) NOT NULL,
  "Identity" character varying(45) ,
  "Email" character varying(45) ,
  "Mobile" character varying(45) ,
  "IsDelete" bit(1) NOT NULL,
  "IsActive" bit(1) ,
  "CreateTime" datetime NOT NULL,
  "UpdateTime" datetime ,
  "Version" int(11) NOT NULL,
  PRIMARY KEY ("Id")
) ;

CREATE TABLE "items" (
  "Id" character(36) NOT NULL,
  "OrderId" character(36) NOT NULL,
  "Index" int(11) NOT NULL,
  "Code" character varying(45) NOT NULL,
  "Name" character varying(45) NOT NULL,
  "Price" float(16,4) NOT NULL,
  "Quantity" float(16,4) NOT NULL,
  "Discount" float(16,4) NOT NULL,
  "Amount" float(16,4) NOT NULL,
  "Unit" character varying(45) ,
  "Version" int(11) NOT NULL,
  PRIMARY KEY ("Id")
) ;

CREATE TABLE "order" (
  "Id" character(36) NOT NULL,
  "BuyerId" character(36) NOT NULL,
  "Number" character varying(45) NOT NULL,
  "Remark" character varying(100) NOT NULL,
  "Status" tinyint(1) NOT NULL,
  "SignState" tinyint(1) ,
  "Amount" float(16,4) NOT NULL,
  "Freight" float(16,4) ,
  "DocId" character(36) ,
  "IsDelete" bit(1) NOT NULL,
  "IsActive" bit(2) not NULL default 0,
  "CreateTime" datetime NOT NULL,
  "UpdateTime" datetime ,
  "Index" INT(11) NOT NULL DEFAULT 0,
  "Version" int(11) NOT NULL,
  PRIMARY KEY ("Id")
) ;

CREATE TABLE "naming_policy_snake_case" (
  "id" character(36) NOT NULL,
  "naming_type" tinyint NOT NULL,
  "create_time" datetime NOT NULL,
  "version" int(11) NOT NULL,
  PRIMARY KEY ("Id")
) ;