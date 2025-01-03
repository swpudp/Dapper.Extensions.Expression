﻿CREATE TABLE "ITEMS"
(
 "ID" VARCHAR(50) NOT NULL,
 "ORDERID" VARCHAR(50) NOT NULL,
 "INDEX" INT NOT NULL,
 "CODE" VARCHAR(45) NOT NULL,
 "NAME" VARCHAR(45) NOT NULL,
 "PRICE" REAL NOT NULL,
 "QUANTITY" REAL NOT NULL,
 "DISCOUNT" REAL NOT NULL,
 "AMOUNT" REAL NOT NULL,
 "UNIT" VARCHAR(45) NULL,
 "VERSION" INT NOT NULL
);
CREATE TABLE "NAMING_POLICY_SNAKE_CASE"
(
 "ID" CHAR(36) NOT NULL,
 "NAMING_TYPE" TINYINT NOT NULL,
 "CREATE_TIME" TIMESTAMP(0) NOT NULL,
 "VERSION" INT NOT NULL
);
CREATE TABLE "ORDER"
(
 "ID" CHAR(36) NOT NULL,
 "BUYERID" CHAR(36) NOT NULL,
 "NUMBER" VARCHAR(45) NOT NULL,
 "REMARK" VARCHAR(100) NOT NULL,
 "STATUS" INT NOT NULL,
 "SIGNSTATE" INT NULL,
 "AMOUNT" REAL NOT NULL,
 "FREIGHT" REAL NULL,
 "DOCID" CHAR(36) NULL,
 "ISDELETE" BIT NOT NULL,
 "ISACTIVE" BIT DEFAULT '0'
 NOT NULL,
 "CREATETIME" TIMESTAMP(0) NOT NULL,
 "UPDATETIME" TIMESTAMP(0) NULL,
 "INDEX" INT DEFAULT 0
 NOT NULL,
 "VERSION" INT NOT NULL,
 "ISENABLE" BIT NULL
);
CREATE TABLE "ATTACHMENT"
(
 "ID" VARCHAR(36) NOT NULL,
 "ORDERID" CHAR(36) NOT NULL,
 "NAME" VARCHAR(45) NOT NULL,
 "EXTEND" VARCHAR(45) NOT NULL,
 "VERSION" INT NOT NULL,
 "ENABLE" INT NULL
);
CREATE TABLE "BUYER"
(
 "ID" VARCHAR(50) NOT NULL,
 "NAME" VARCHAR(45) NOT NULL,
 "TYPE" TINYINT NOT NULL,
 "CODE" VARCHAR(45) NOT NULL,
 "IDENTITY" VARCHAR(45) NULL,
 "EMAIL" VARCHAR(45) NULL,
 "MOBILE" VARCHAR(45) NULL,
 "ISDELETE" NUMBER(1,0) NOT NULL,
 "ISACTIVE" NUMBER(1,0) NULL,
 "CREATETIME" TIMESTAMP(0) NOT NULL,
 "UPDATETIME" TIMESTAMP(0) NULL,
 "VERSION" INT NOT NULL
);