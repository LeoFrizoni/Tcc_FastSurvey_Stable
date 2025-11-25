def create_connection():
    import sqlite3
    from sqlite3 import Error

    conn = None
    try:
        conn = sqlite3.connect("sentiment_analysis.db")
        print("Conexão com o banco de dados SQLite bem-sucedida")
    except Error as e:
        print(f"O erro '{e}' ocorreu")

    return conn